using System.Net;
using Application.Common.Enums;

namespace UnitTest
{
    public class FetchOptions
    {
        public ErrorCode? ErrorCode { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.OK;
    }
}