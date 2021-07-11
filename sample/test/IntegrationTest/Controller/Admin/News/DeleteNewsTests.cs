using System.Net.Http;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Features.News.Commands.DeleteNews;
using IntegrationTest.Utilities;
using MarkopTest.IntegrationTest;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller.Admin.News
{
    public class DeleteNewsTests : AppFactory
    {
        public DeleteNewsTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopIntegrationTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1, ErrorCode.InvalidInput)]
        public async Task DeleteNews(int newsId, ErrorCode? errorCode = null)
        {
            Client ??= await Host.OwnerClient();

            var data = new DeleteNewsCommand
            {
                NewsId = newsId
            };

            await PostJsonAsync(data, Client, new FetchOptions
            {
                ErrorCode = errorCode
            });
        }
    }
}