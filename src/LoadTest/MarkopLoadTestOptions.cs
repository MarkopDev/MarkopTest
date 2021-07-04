using System.Net.Http;

namespace MarkopTest.LoadTest
{
    public class MarkopLoadTestOptions
    {
        public string ChartColor { get; set; } = "";
        public HttpClient DefaultHttpClient { get; set; }
        public bool OpenResultAfterFinished { get; set; } = true;
    }
}