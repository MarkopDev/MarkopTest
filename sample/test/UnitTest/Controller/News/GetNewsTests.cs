using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Features.News.Queries.GetNews;
using MarkopTest.UnitTest;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.Controller.News
{
    public class GetNewsTests : AppFactory
    {
        public GetNewsTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopUnitTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
        [InlineData(1)]
        [InlineData(-1, ErrorCode.InvalidInput)]
        public async Task<GetNewsViewModel> GetNews(int newsId, ErrorCode? errorCode = null)
        {
            var data = new GetNewsQuery
            {
                NewsId = newsId
            };

            var response = await Fetch(data, Client, new FetchOptions
            {
                ErrorCode = errorCode
            });

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<GetNewsViewModel>()
                : null;
        }
    }
}