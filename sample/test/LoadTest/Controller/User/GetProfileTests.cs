using System.Threading.Tasks;
using Application.Features.User.Queries.GetProfile;
using IntegrationTest.Handlers;
using MarkopTest.LoadTest;
using Xunit;
using Xunit.Abstractions;

namespace LoadTest.Controller.User
{
    public class GetProfileTests : AppFactory
    {
        public GetProfileTests(ITestOutputHelper outputHelper) : base(outputHelper,
            new LoadTestOptions())
        {
        }

        [Fact]
        [UserHandler]
        public async Task GetProfile()
        {
            var data = new GetProfileQuery();

            await PostJsonAsync(data);
        }
    }
}