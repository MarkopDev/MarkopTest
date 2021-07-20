using System.Net.Http;

namespace MarkopTest.IntegrationTest
{
    public class IntegrationTestOptions
    {
        public bool LogResponse { get; set; } = true;
        public HttpClient DefaultHttpClient { get; set; }
    }
}