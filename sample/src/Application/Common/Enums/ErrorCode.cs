namespace Application.Common.Enums;

public enum ErrorCode : short
{
    Unexpected,
    Unauthorized,
    InvalidInput,
    Duplicate,
    NotFound,
    AccessDenied,
    PhoneNumberAlreadyConfirmed,
    NeedConfirmPhoneNumber,
    NotEnoughFreeCredit
}