using System.Net.Http;

namespace MarkopTest.LoadTest
{
    public class MarkopLoadTestOptions
    {
        public string ChartColor { get; set; } = "";
        public int SyncRequestCount { get; set; } = 50;
        public HttpClient DefaultHttpClient { get; set; }
        public int AsyncRequestCount { get; set; } = 1000;
        public bool OpenResultAfterFinished { get; set; } = true;
    }
}