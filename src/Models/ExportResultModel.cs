namespace MarkopTest.Models
{
    public class ExportResultModel
    {
        public string OS { get; set; }
        public string ApiUrl { get; set; }
        public string RamSize { get; set; }
        public string CpuName { get; set; }
        public string ChartColor { get; set; }
        public int SyncRequestCount { get; set; }
        public int AsyncRequestCount { get; set; }
        public string SyncSummaryRange { get; set; }
        public string AsyncSummaryRange { get; set; }
        public string SyncMemorySamples { get; set; }
        public string AsyncMemorySamples { get; set; }
        public string SyncResponseStatus { get; set; }
        public string AsyncResponseStatus { get; set; }
        public string SyncMaxResponseTime { get; set; }
        public string SyncMinResponseTime { get; set; }
        public string SyncAvgResponseTime { get; set; }
        public string AsyncMaxResponseTime { get; set; }
        public string AsyncMinResponseTime { get; set; }
        public string AsyncAvgResponseTime { get; set; }
        public string SyncTimesIterationsJsArray { get; set; }
        public string AsyncTimesIterationsJsArray { get; set; }
        public string SyncTimesDistributionJsArray { get; set; }
        public string AsyncTimesDistributionJsArray { get; set; }
    }
}