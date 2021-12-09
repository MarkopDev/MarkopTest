using System.Threading.Tasks;
using Application.Features.User.Queries.GetProfile;
using IntegrationTest.Handlers;
using Xunit;
using Xunit.Abstractions;

namespace LoadTest.Controller.User;

public class GetProfileTests : AppFactory
{
    public GetProfileTests(ITestOutputHelper outputHelper) : base(outputHelper)
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