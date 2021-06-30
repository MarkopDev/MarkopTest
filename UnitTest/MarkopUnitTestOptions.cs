using System.Net.Http;

namespace MarkopTest.UnitTest
{
    public class MarkopUnitTestOptions
    {
        public bool LogResponse { get; set; } = true;
        public HttpClient DefaultHttpClient { get; set; }
    }
}