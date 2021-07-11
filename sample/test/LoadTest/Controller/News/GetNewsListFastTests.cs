using System.Net.Http;
using System.Threading.Tasks;
using Application.Features.News.Queries.GetNewsListFast;
using IntegrationTest.Utilities;
using MarkopTest.LoadTest;
using Xunit;
using Xunit.Abstractions;

namespace LoadTest.Controller.News
{
    public class GetNewsListFastTests : AppFactory
    {
        public GetNewsListFastTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopLoadTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
        [InlineData(1, 100)]
        public async Task GetNewsListFast(int pageNumber, int pageSize)
        {
            Client ??= await Host.UserClient();
            
            var data = new GetNewsListFastQuery
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
            };

            await PostJsonAsync(data, Client);
        }
    }
}