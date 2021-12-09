using Application.Common.Enums;

namespace Application.Common.Models;

public struct Error
{
    public string Message { get; set; }
    public ErrorCode Code { get; set; }
}