using System.Net;

namespace MarkopTest.Models
{
    public class RequestInfo
    {
        public long ResponseTime { get; set; }
        public HttpStatusCode ResponseStatus { get; set; }
    }
}