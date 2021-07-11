using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Features.News.Commands.CreateNews;
using IntegrationTest.Utilities;
using MarkopTest.IntegrationTest;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller.Admin.News
{
    public class CreateNewsTests : AppFactory
    {
        public CreateNewsTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopIntegrationTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
        [InlineData("New Title")]
        [InlineData(null, ErrorCode.InvalidInput)]
        public async Task<CreateNewsViewModel> CreateNews(string title, ErrorCode? errorCode = null)
        {
            Client ??= await Host.OwnerClient();

            var data = new CreateNewsCommand
            {
                Title = title
            };

            var response = await PostJsonAsync(data, Client, new FetchOptions
            {
                ErrorCode = errorCode
            });

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<CreateNewsViewModel>()
                : null;
        }
    }
}