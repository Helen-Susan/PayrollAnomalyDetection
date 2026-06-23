//this service is to implement upload service where the csv file is uploaded and it is is converted to table and stored in sql server and it also calls the fabric ingestion service to ingest the data into fabric kusto database.analytical function is also called from here.
using anamoly_detection_api.Models.DTO;
using CsvHelper;
using CsvHelper.Configuration;
using Dapper;//sql functionality
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static anamoly_detection_api.Models.DTO.AnomalyFlagDto;
namespace anamoly_detection_api.Services
{
    public class UploadService : IUploadService 
    {   
        #region private variables

        // We need the connection string to connect to SQL Server.
        // It comes from appsettings.json and is injected here automatically.

        private readonly string _connectionString;
        private readonly ICycleInformationService _cycleInformationService;
        private readonly IAnalyticsPersistenceService _analyticsPersistenceService;
        #endregion private variables
        List<string> headers;
        List<Dictionary<string, string>> dataRows;
        private readonly IFabricIngestionService _fabricIngestionService;


        #region constructor
        public UploadService(IConfiguration configuration,ICycleInformationService cycleInformationService,IAnalyticsPersistenceService analyticsPersistenceService,IFabricIngestionService fabricIngestionService)
        {
            _connectionString =
                configuration.GetConnectionString(
                    "DefaultSQLConnection")!;

            _cycleInformationService =
                cycleInformationService;

            _analyticsPersistenceService =
                analyticsPersistenceService;
            _fabricIngestionService = fabricIngestionService;
        }
        #endregion constructor

        #region public methods
        public async Task<UploadResponseDto> UploadAsync(IFormFile file)
        {


            // Check if the file is empty or missing
            if (file == null || file.Length == 0)
            {
                return new UploadResponseDto
                {
                    Success = false,
                    Message = "No file was uploaded or the file is empty."
                };
            }

            // Check if the file extension is .csv
            // Path.GetExtension gets the ".csv" part of the filename
            var extension = Path.GetExtension(file.FileName);
            if (!extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return new UploadResponseDto
                {
                    Success = false,
                    Message = "Only .csv files are allowed."
                };
            }

            //  READ THE CSV FILE ─────────────────────────────────────

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim
            };

            // Open the file as a stream and read it
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                // Read the header row
                await csv.ReadAsync();
                csv.ReadHeader();

                headers = csv.HeaderRecord!.ToList();

                // Read all data rows
                dataRows = new List<Dictionary<string, string>>();

                while (await csv.ReadAsync())
                {
                    var row = new Dictionary<string, string>();

                    foreach (var header in headers)
                    {
                        row[header] = csv.GetField(header) ?? string.Empty;
                    }

                    dataRows.Add(row);
                }
            }

            // Check if we actually got any headers
            if (headers.Count == 0)
            {
                return new UploadResponseDto
                {
                    Success = false
                };
            }

            // ── STEP 3: PREPARE THE TABLE NAME AND COLUMN NAMES ──────────────
            var rawTableName = Path.GetFileNameWithoutExtension(file.FileName);
            var tableName = SanitizeName(rawTableName, "CsvTable");
            var fileId = tableName
                            .ToLower()
                            .Replace("_", "")
                            .Replace("payroll", "")
                            .ToUpper();
            // Also clean each column name
            var cleanColumns = headers.Select(h => SanitizeName(h, "Column")).ToList();

            // CREATE TABLE AND INSERT DATA IN SQL SERVER ────────────

            // Open a connection to SQL Server
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Build the CREATE TABLE SQL command dynamically
                // using the column names we got from the CSV headers
                var createTableSql = BuildCreateTableSql(tableName, cleanColumns);
                //check whether the table exists or not
                var tableExistsQuery = @"
                                        SELECT COUNT(*)
                                        FROM INFORMATION_SCHEMA.TABLES
                                        WHERE TABLE_NAME = @TableName";

                var tableExists = await connection.ExecuteScalarAsync<int>(
                    tableExistsQuery,
                    new { TableName = tableName },
                    transaction
                );

                if (tableExists > 0)
                {
                    transaction.Rollback();

                    return new UploadResponseDto
                    {
                        Success = false,
                        Message = $"Table '{tableName}' already exists."
                    };
                }

                // Run the CREATE TABLE command
                await connection.ExecuteAsync(createTableSql, transaction: transaction);

                // Now insert all the rows using SqlBulkCopy (fast bulk insert)
                await InsertRowsAsync(connection, transaction, tableName, headers, cleanColumns, dataRows);
                var insertFileQuery = @"
                                        INSERT INTO UploadedFiles
                                        (
                                            Id,
                                            FileName,
                                            TableName
                                        )
                                        VALUES
                                        (
                                            @Id,
                                            @FileName,
                                            @TableName
                                        )";

                await connection.ExecuteAsync(insertFileQuery, new
                {
                    Id = fileId,
                    FileName = file.FileName,
                    TableName = tableName
                },
                transaction
                );

                transaction.Commit();
                var fabricSuccess =
        await _fabricIngestionService
            .IngestPayrollAsync(
                dataRows,
                fileId);





                // Return a success response
                return new UploadResponseDto
                {
                    Success = true,
                    Message = $"Table '{tableName}' was created and {dataRows.Count} row(s) were inserted successfully.",
                    TableName = tableName,
                    Columns = cleanColumns,
                    RowsInserted = dataRows.Count,
                    Fileid = fileId,
                    Id = fileId,    
                    uploadedAt = DateTime.UtcNow,
                    FileName = file.FileName,
                    status = "success"


                };
            }
            catch (Exception )
            {
                if (transaction.Connection != null)
                {
                    transaction.Rollback();
                }

                throw;
            }
        }
        #endregion public methods

            #region private methods
            // method :to create the table in sql server with the column names from csv header
        private static string BuildCreateTableSql(string tableName, List<string> columns)
        {
            // StringBuilder is used to build a long string piece by piece
            var sb = new StringBuilder();

            sb.AppendLine($"CREATE TABLE [{tableName}] (");

            // Add an auto-increment ID column as the primary key
            sb.AppendLine("    [Id]         INT       IDENTITY(1,1) PRIMARY KEY,");

            // Add one column for each CSV header
            // All columns are NVARCHAR(MAX) — they hold any text value
            foreach (var col in columns)
            {
                sb.AppendLine($"    [{col}]  NVARCHAR(MAX) NULL,");
            }

            // Add a timestamp column so we know when the row was inserted
            sb.AppendLine(" [UploadedAt] DATETIME2 DEFAULT GETUTCDATE(),");
            sb.AppendLine("[Anomaly_Flag] NVARCHAR(50) NULL DEFAULT 'Normal',");
            sb.AppendLine("[SeverityLevel] NVARCHAR(50) NULL,");
            sb.AppendLine("[ReviewStatus] NVARCHAR(50) NULL,");
            sb.AppendLine("[AnomalyType] NVARCHAR(100) NULL,");

            sb.Append(");");

            return sb.ToString();
        }

        // ── HELPER METHOD 2: Insert all CSV rows into the table ───────────────
        
        private static async Task InsertRowsAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            string tableName,
            List<string> originalHeaders,   // original CSV headers (e.g. "First Name")
            List<string> cleanColumns,      // sanitised SQL columns  (e.g. "First_Name")
            List<Dictionary<string, string>> dataRows)
        {
            // DataTable is an in-memory table — we fill it first, then send to SQL
            var dataTable = new DataTable();

            // Add columns to the DataTable (matching our SQL table columns)
            foreach (var col in cleanColumns)
            {
                dataTable.Columns.Add(col, typeof(string));
            }

            // Add each CSV row into the DataTable
            foreach (var row in dataRows)
            {
                var dataRow = dataTable.NewRow();

                for (int i = 0; i < originalHeaders.Count; i++)
                {
                    // Get value using original header, store it under the clean column name
                    dataRow[cleanColumns[i]] = row.TryGetValue(originalHeaders[i], out var val)
                        ? val
                        : string.Empty;
                }

                dataTable.Rows.Add(dataRow);
            }

            // SqlBulkCopy sends all rows to SQL Server in one fast operation
            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            {
                DestinationTableName = $"[{tableName}]",
                BatchSize = 500  // Send 500 rows at a time
            };

            // Map each DataTable column to the matching SQL table column
            foreach (DataColumn col in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
            }

            // Execute the bulk insert
            await bulkCopy.WriteToServerAsync(dataTable);
        }

        // method:to clean the table name and column names by replacing spaces and special characters with underscores, and ensuring they are valid SQL identifiers
        private static string SanitizeName(string input, string fallback)
        {
            if (string.IsNullOrWhiteSpace(input))
                return fallback;

            // Replace anything that is NOT a letter, digit, or underscore with _
            var clean = Regex.Replace(input.Trim(), @"[^a-zA-Z0-9_]", "_");

            // Remove multiple underscores in a row (e.g. "First___Name" → "First_Name")
            clean = Regex.Replace(clean, @"_+", "_").Trim('_');

            // If the name starts with a number, add a prefix
            if (string.IsNullOrEmpty(clean))
                return fallback;

            if (char.IsDigit(clean[0]))
                clean = "Col_" + clean;

            // Limit to 100 characters (SQL Server allows 128 max)
            return clean.Length > 100 ? clean[..100] : clean;
        }
    }
        #endregion private methods
}