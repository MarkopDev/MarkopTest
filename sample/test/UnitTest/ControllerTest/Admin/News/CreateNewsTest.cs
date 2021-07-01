using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Features.News.Commands.CreateNews;
using MarkopTest.UnitTest;
using UnitTest.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest.ControllerTest.Admin.News
{
    public class CreateNewsTest : AppFactory
    {
        public CreateNewsTest(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopUnitTestOptions {DefaultHttpClient = client})
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

            var response = await Fetch(data, Client, new FetchOptions
            {
                ErrorCode = errorCode
            });

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<CreateNewsViewModel>()
                : null;
        }
    }
}