namespace MarkopTest.Models
{
    public class ExportResultModel
    {
        public string SyncSummaryRange { get; set; }
        public string AsyncSummaryRange { get; set; }
        public string SyncResponseStatus { get; set; }
        public string AsyncResponseStatus { get; set; }
        public string SyncTimesIterationsJsArray { get; set; }
        public string AsyncTimesIterationsJsArray { get; set; }
        public string SyncTimesDistributionJsArray { get; set; }
        public string AsyncTimesDistributionJsArray { get; set; }
    }
}