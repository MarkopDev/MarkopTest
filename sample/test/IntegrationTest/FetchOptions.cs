using System.Net;
using Application.Common.Enums;

namespace IntegrationTest;

public class FetchOptions
{
    public ErrorCode? ErrorCode { get; set; }
    public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.OK;
}