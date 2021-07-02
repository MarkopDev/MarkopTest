using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.DTOs.User;
using Application.Features.User.Queries.GetProfile;
using MarkopTest.UnitTest;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.Controller.User
{
    public class GetProfileTests : AppFactory
    {
        public GetProfileTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopUnitTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
        [InlineData(null)]
        public async Task<ProfileDto> GetProfile(ErrorCode? errorCode = null)
        {
            var data = new GetProfileQuery();

            var response = await Fetch(data, Client, new FetchOptions
            {
                ErrorCode = errorCode
            });

            return response.IsSuccessStatusCode
                ? (await response.Content.ReadFromJsonAsync<GetProfileViewModel>())?.Profile
                : null;
        }

        [Fact]
        public async Task GetProfile_UnauthorizedUser()
        {
            Client = Host.GetTestClient();
            await GetProfile(ErrorCode.Unauthorized);
        }
    }
}