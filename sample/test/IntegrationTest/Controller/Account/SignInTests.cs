using System.Net.Http;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Features.Account.Commands.SignIn;
using MarkopTest.IntegrationTest;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller.Account
{
    public class SignInTests : AppFactory
    {
        public SignInTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopIntegrationTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
        [InlineData("TestUser@Markop.com", LoginType.Email, "TestPassword")]
        [InlineData(null, LoginType.Email, "TestPassword1", ErrorCode.InvalidInput)]
        [InlineData("TestUser@Markop.com", LoginType.Email, null, ErrorCode.InvalidInput)]
        [InlineData("TestTest@Markop.com", LoginType.Email, "TestPassword", ErrorCode.InvalidInput)]
        [InlineData("TestUser@Markop.com", LoginType.Email, "TestPassword1", ErrorCode.InvalidInput)]
        public async Task SignIn_ShouldWorkCorrectly(string login, LoginType loginType, string password, ErrorCode? errorCode = null)
        {
            var data = new SignInCommand
            {
                Login = login,
                Type = loginType,
                Password = password
            };

            await Post(data, fetchOptions: new FetchOptions
            {
                ErrorCode = errorCode
            });
        }
    }
}
