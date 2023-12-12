using Application.Features.User.Queries.GetProfile;
using IntegrationTest.Handlers;
using MarkopTest.Attributes;
using Xunit.Abstractions;

namespace LoadTest.Controller.User;

public class GetProfileTests : AppFactory
{
    public GetProfileTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    [UserHandler]
    [Endpoint("User/GetProfile")]
    public void GetProfile()
    {
        var data = new GetProfileQuery();

        PostJson(data);
    }
}