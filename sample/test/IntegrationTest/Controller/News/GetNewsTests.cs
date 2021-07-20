using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Features.News.Queries.GetNews;
using IntegrationTest.Handlers;
using MarkopTest.IntegrationTest;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller.News
{
    public class GetNewsTests : AppFactory
    {
        public GetNewsTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new IntegrationTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
        [UserHandler]
        [InlineData(1)]
        [InlineData(-1, ErrorCode.InvalidInput)]
        public async Task<GetNewsViewModel> GetNews(int newsId, ErrorCode? errorCode = null)
        {
            var data = new GetNewsQuery
            {
                NewsId = newsId
            };

            var response = await PostJsonAsync(data, new FetchOptions
            {
                ErrorCode = errorCode
            });

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<GetNewsViewModel>()
                : null;
        }
    }
}