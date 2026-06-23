//this code defines a service called `FabricIngestionService` that implements the `IFabricIngestionService` interface. The primary responsibility of this service is to ingest payroll data into a Fabric Kusto database.The dara get stored to payrollrecodrds 0f kustodatabase.
using anamoly_detection_api.Services;
using Kusto.Data;
using Kusto.Data.Net.Client;

namespace anamoly_detection_api.Services
{ }
    public class FabricIngestionService
        : IFabricIngestionService
{
        #region private fields
        private readonly IConfiguration _configuration;
        #endregion
        #region Constructor
        public FabricIngestionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion
        #region public methods
        public async Task<bool> IngestPayrollAsync(List<Dictionary<string, string>> dataRows, string cycleId)
        {
            try
            {
                var queryUri =
                    _configuration["FabricKusto:QueryUri"];

                var database =
                    _configuration["FabricKusto:Database"];

                var tableName = "PayrollRecords";


                var kcsb =
                    new KustoConnectionStringBuilder(queryUri)
                        .WithAadUserPromptAuthentication();

                using var adminClient =
                    KustoClientFactory
                        .CreateCslAdminProvider(kcsb);

                var command =
                    BuildIngestionCommand(
                        dataRows,
                        cycleId,
                        tableName!);

                await Task.Run(() =>
                {
                    adminClient.ExecuteControlCommand(
                        database,
                        command);
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Fabric ingestion failed: {ex}");

                return false;
            }
        }
        #endregion
        #region private methods
        private string BuildIngestionCommand(List<Dictionary<string, string>> rows, string cycleId, string tableName)
        {
            var lines = new List<string>();

            foreach (var row in rows)
            {
                lines.Add(
                    string.Join(",",
                        Escape(cycleId),
                        Escape(Get(row, "Employee_ID")),
                        Escape(Get(row, "Department")),
                        Escape(Get(row, "Location")),
                        Escape(Get(row, "EmploymentType")),
                        Escape(Get(row, "Pay_Period")),
                        Get(row, "Base_Salary"),
                        Get(row, "Bonus"),
                        Get(row, "Overtime"),
                        Get(row, "Deduction"),
                        Get(row, "Net_Pay"),
                        Escape(Get(row, "PaymentStatus")),
                        Escape(Get(row, "ApprovalStatus")),
                        Escape(Get(row, "Bank_Account")),
                        Escape(Get(row, "Tax_ID")),
                        Escape(Get(row, "Join_Date")),

                        // Fabric will determine anomalies
                        "Normal"
                    ));
            }

            return
                        $"""
                        .ingest inline into table {tableName} <|
                        {string.Join(Environment.NewLine, lines)}
                        """;
        }

        private static string Get(Dictionary<string, string> row, string key)
        {
            return row.TryGetValue(key, out var value)
                ? value
                : string.Empty;
        }

        private static string Escape(string value)
        {
            return value
                .Replace(",", " ")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }
    }
        #endregion

