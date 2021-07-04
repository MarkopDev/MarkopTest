using System.Net.Http;
using System.Threading.Tasks;
using Application.Features.User.Queries.GetProfile;
using IntegrationTest.Utilities;
using MarkopTest.LoadTest;
using Xunit;
using Xunit.Abstractions;

namespace LoadTest.Controller.User
{
    public class GetProfileTests : AppFactory
    {
        public GetProfileTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopLoadTestOptions {DefaultHttpClient = client})
        {
        }

        [Fact]
        public async Task GetProfile()
        {
            Client ??= await Host.UserClient();

            var data = new GetProfileQuery();

            await Fetch(data, Client);
        }
    }
}