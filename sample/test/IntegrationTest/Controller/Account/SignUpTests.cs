using System.Net.Http;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Features.Account.Commands.SignUp;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller.Account
{
    public class SignUpTests : AppFactory
    {
        public SignUpTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper, client)
        {
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
        public async Task SignUp(string phoneNumber, string password, string firstname, 
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

            await PostJsonAsync(data, new FetchOptions
            {
                ErrorCode = errorCode
            });
        }
    }
}
