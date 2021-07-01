using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Features.Account.Commands.SignOut;
using MarkopTest.UnitTest;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.ControllerTest.Account
{
    public class SignOutTest : AppFactory
    {
        public SignOutTest(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopUnitTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
        [InlineData(null)]
        public async Task SignOut(ErrorCode? errorCode = null)
        {
            Client = await GetDefaultClient();
            
            var data = new SignOutCommand();

            var response = await Fetch(data, Client, new FetchOptions
            {
                ErrorCode = errorCode
            });

            Client.DefaultRequestHeaders.Add("Cookie", response.Headers.GetValues("Set-Cookie").ToArray()[0]);

            await Fetch(data, Client, new FetchOptions
            {
                ErrorCode = null,
                HttpStatusCode = HttpStatusCode.Unauthorized
            });
        }
    }
}