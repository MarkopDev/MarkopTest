using System.Net;
using System.Net.Http;
using Application.Common.Enums;
using Application.Features.Account.Commands.SignIn;
using Application.Features.Account.Commands.SignOut;
using Application.Features.Account.Commands.SignUp;
using IntegrationTest.Handlers;
using MarkopTest.Attributes;
using MarkopTest.Handler;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller;

[Endpoint("[controller]/[action]")]
public class AccountTests : AppFactory
{
    public AccountTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper, client)
    {
    }

    [Theory]
    [InlineData("TestUser@Markop.com", LoginType.Email, "TestPassword")]
    [InlineData(null, LoginType.Email, "TestPassword1", ErrorCode.InvalidInput)]
    [InlineData("TestUser@Markop.com", LoginType.Email, null, ErrorCode.InvalidInput)]
    [InlineData("TestTest@Markop.com", LoginType.Email, "TestPassword", ErrorCode.InvalidInput)]
    [InlineData("TestUser@Markop.com", LoginType.Email, "TestPassword1", ErrorCode.InvalidInput)]
    public void SignIn(string login, LoginType loginType, string password, ErrorCode? errorCode = null)
    {
        var data = new SignInCommand
        {
            Login = login,
            Type = loginType,
            Password = password
        };

        PostJson(data, new FetchOptions
        {
            ErrorCode = errorCode
        });
    }

    [Theory]
    [UserHandler]
    [InlineData(null)]
    public void SignOut(ErrorCode? errorCode = null)
    {
        var data = new SignOutCommand();

        PatchJson(data, new FetchOptions
        {
            ErrorCode = errorCode
        });

        PatchJson(data,
            new FetchOptions
            {
                ErrorCode = null,
                HttpStatusCode = HttpStatusCode.Unauthorized
            },
            new TestHandlerOptions
            {
                BeforeRequest = false
            });
    }

    [Theory]
    [InlineData("0098 12345678", "TestPassword", "Test", "Test")]
    [InlineData(null, "TestPassword", "Test", "Test", "Web Test", ErrorCode.InvalidInput)]
    [InlineData("0098 12345678", null, "Test", "Test", "Web Test", ErrorCode.InvalidInput)]
    [InlineData("0098 12345678", "ssword", "Test", "Test", "Web Test", ErrorCode.InvalidInput)]
    [InlineData("Test@.COM", "TestPassword", "Test", "Test", "Web Test", ErrorCode.InvalidInput)]
    [InlineData("98 12345678", "TestPassword", "Test", "Test", "Web Test", ErrorCode.InvalidInput)]
    [InlineData("0098 12345678", "TestPassword", null, "Test", "Web Test", ErrorCode.InvalidInput)]
    [InlineData("0098 12345678", "TestPassword", "Test", null, "Web Test", ErrorCode.InvalidInput)]
    [InlineData("009812345678", "TestPassword", "Test", "Test", "Web Test", ErrorCode.InvalidInput)]
    public void SignUp(string phoneNumber, string password, string firstname,
        string lastName, string platform = "Web Test", ErrorCode? errorCode = null)
    {
        var data = new SignUpCommand
        {
            PhoneNumber = phoneNumber,
            Password = password,
            FirstName = firstname,
            LastName = lastName,
            Platform = platform
        };

        PostJson(data, new FetchOptions
        {
            ErrorCode = errorCode
        });
    }
}