using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Features.Account.Commands.SignOut;
using IntegrationTest.Handlers;
using MarkopTest.IntegrationTest;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller.Account
{
    public class SignOutTests : AppFactory
    {
        public SignOutTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new IntegrationTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
        [UserHandler]
        [InlineData(null)]
        public async Task SignOut(ErrorCode? errorCode = null)
        {
            var data = new SignOutCommand();

            var response = await PostJsonAsync(data, new FetchOptions
            {
                ErrorCode = errorCode
            });
            
            GetClient().DefaultRequestHeaders.Add("Cookie", response.Headers.GetValues("Set-Cookie").ToArray()[0]);

            await PostJsonAsync(data, new FetchOptions
            {
                ErrorCode = null,
                HttpStatusCode = HttpStatusCode.Unauthorized
            });
        }
    }
}