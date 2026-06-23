namespace anamoly_detection_api.Models.DTO
{

    // This class holds the response we send back after the upload
    public class UploadResponseDto
    {
        // true  = upload worked fine
        // false = something went wrong
        public bool Success { get; set; }

        // A simple message telling what happened
        // Example: "Table 'employees' created with 10 rows."
        public string Message { get; set; } = string.Empty;

        // The name of the SQL table that was created from the CSV
        public string TableName { get; set; } = string.Empty;

        // The column names that were found in the CSV header row
        // Example: ["Name", "Age", "Department"]
        public List<string> Columns { get; set; } = new List<string>();

        // How many rows were inserted into the table
        public int RowsInserted { get; set; }
        public string Id { get; set; } = string.Empty;

        public string Fileid { get; set; }
        public DateTime uploadedAt { get; set; }
        public string FileName { get; set; } 
        public string status { get; set; }
    }
    
}


