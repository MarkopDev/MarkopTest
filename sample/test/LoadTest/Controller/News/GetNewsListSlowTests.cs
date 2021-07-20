using System.Threading.Tasks;
using Application.Features.News.Queries.GetNewsListSlow;
using IntegrationTest.Handlers;
using MarkopTest.LoadTest;
using Xunit;
using Xunit.Abstractions;

namespace LoadTest.Controller.News
{
    public class GetNewsListSlowTests : AppFactory
    {
        public GetNewsListSlowTests(ITestOutputHelper outputHelper) : base(outputHelper,
            new LoadTestOptions())
        {
        }

        [Theory]
        [UserHandler]
        [InlineData(1, 100)]
        public async Task GetNewsListSlow(int pageNumber, int pageSize)
        {
            var data = new GetNewsListSlowQuery
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
            };

            await PostJsonAsync(data);
        }
    }
}