using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.DTOs.User;
using Application.Features.User.Queries.GetProfile;
using IntegrationTest.Handlers;
using MarkopTest.IntegrationTest;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller.User
{
    public class GetProfileTests : AppFactory
    {
        public GetProfileTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new IntegrationTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
        [UserHandler]
        [InlineData(null)]
        public async Task<ProfileDto> GetProfile(ErrorCode? errorCode = null)
        {
            var data = new GetProfileQuery();

            var response = await PostJsonAsync(data, new FetchOptions
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
            await GetProfile(ErrorCode.Unauthorized);
        }
    }
}