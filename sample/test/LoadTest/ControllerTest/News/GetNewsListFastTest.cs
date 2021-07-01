using System.Net.Http;
using System.Threading.Tasks;
using Application.Features.News.Queries.GetNewsListFast;
using MarkopTest.LoadTest;
using UnitTest.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace LoadTest.ControllerTest.News
{
    public class GetNewsListFastTest : AppFactory
    {
        public GetNewsListFastTest(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
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

            await Fetch(data, Client);
        }
    }
}