using System.Net.Http;

namespace MarkopTest.IntegrationTest
{
    public class MarkopIntegrationTestOptions
    {
        public bool LogResponse { get; set; } = true;
        public HttpClient DefaultHttpClient { get; set; }
    }
}