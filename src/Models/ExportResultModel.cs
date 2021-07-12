namespace MarkopTest.Models
{
    public class ExportResultModel
    {
        public string OS { get; set; }
        public long RamSize { get; set; }
        public string ApiUrl { get; set; }
        public string CpuName { get; set; }
        public string ChartColor { get; set; }
        public int SyncRequestCount { get; set; }
        public int AsyncRequestCount { get; set; }
        public long SyncMaxResponseTime { get; set; }
        public long SyncMinResponseTime { get; set; }
        public long SyncAvgResponseTime { get; set; }
        public long[] SyncMemorySamples { get; set; }
        public long[] AsyncMemorySamples { get; set; }
        public long AsyncMaxResponseTime { get; set; }
        public long AsyncMinResponseTime { get; set; }
        public long AsyncAvgResponseTime { get; set; }
        public long[][] SyncTimesIterations { get; set; }
        public long[][] AsyncTimesIterations { get; set; }
        public long[][] SyncTimesDistribution { get; set; }
        public long[][] AsyncTimesDistribution { get; set; }
        public ResponseStatusResult[] SyncResponseStatus { get; set; }
        public ResponseStatusResult[] AsyncResponseStatus { get; set; }
        public ResponseTimeSummaryResult[] SyncTimeSummaryRange { get; set; }
        public ResponseTimeSummaryResult[] AsyncTimeSummaryRange { get; set; }
    }
}