namespace anamoly_detection_api.Models.Confriguration
{
    public class FabricKustoSettings
    {
        public string QueryUri { get; set; } = string.Empty;

        public string IngestUri { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public string TableName { get; set; } = string.Empty;
    }
}
