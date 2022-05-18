using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.DTOs.User;
using Application.Features.User.Queries.GetProfile;
using IntegrationTest.Handlers;
using MarkopTest.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller;

[Endpoint("[controller]/[action]")]
public class UserTests : AppFactory
{
    public UserTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper, client)
    {
    }
    
    [Theory]
    [UserHandler]
    [InlineData(null)]
    public async Task<ProfileDto> GetProfile(ErrorCode? errorCode = null)
    {
        var data = new GetProfileQuery();

        var response =  PostJson(data, new FetchOptions
        {
            ErrorCode = errorCode
        });

        return response.IsSuccessStatusCode
            ? (await response.Content.ReadFromJsonAsync<GetProfileViewModel>())?.Profile
            : null;
    }

    [Endpoint("[controller]/GetProfile")]
    [Fact]
    public async Task GetProfile_UnauthorizedUser()
    {
        await GetProfile(ErrorCode.Unauthorized);
    }
}