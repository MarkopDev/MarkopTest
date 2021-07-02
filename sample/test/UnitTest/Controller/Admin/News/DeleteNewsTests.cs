using System.Net.Http;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Features.News.Commands.DeleteNews;
using MarkopTest.UnitTest;
using UnitTest.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.Controller.Admin.News
{
    public class DeleteNewsTest : AppFactory
    {
        public DeleteNewsTest(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopUnitTestOptions {DefaultHttpClient = client})
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

            await Fetch(data, Client, new FetchOptions
            {
                ErrorCode = errorCode
            });
        }
    }
}