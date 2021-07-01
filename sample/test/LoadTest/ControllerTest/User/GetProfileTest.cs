using System.Net.Http;
using System.Threading.Tasks;
using Application.Features.User.Queries.GetProfile;
using MarkopTest.LoadTest;
using UnitTest.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace LoadTest.ControllerTest.User
{
    public class GetProfileTest : AppFactory
    {
        public GetProfileTest(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
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