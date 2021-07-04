using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Features.News.Commands.EditNews;
using IntegrationTest.Utilities;
using MarkopTest.IntegrationTest;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller.Admin.News
{
    public class EditNewsTests : AppFactory
    {
        public EditNewsTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopIntegrationTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
        [InlineData(1,"Edited Title", "Edited Content", "Preview Edited", false)]
        [InlineData(1, "Edited Title", null, "Preview Edited", false, ErrorCode.InvalidInput)]
        [InlineData(1, "Edited Title", "Edited Content", null, false, ErrorCode.InvalidInput)]
        [InlineData(1, null, "Edited Content", "Preview Edited", false, ErrorCode.InvalidInput)]
        [InlineData(-1, "Edited Title", "Edited Content", "Preview Edited", false, ErrorCode.InvalidInput)]
        public async Task<EditNewsViewModel> EditNews(int newsId, string title, string content, string preview, bool isHidden, ErrorCode? errorCode = null)
        {
            Client ??= await Host.OwnerClient();
            
            var data = new EditNewsCommand
            {
                Title = title,
                NewsId = newsId,
                Content = content,
                Preview = preview,
                IsHidden = isHidden
            };

            var response = await Post(data, Client, new FetchOptions
            {
                ErrorCode = errorCode
            });

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<EditNewsViewModel>()
                : null;
        }
    }
}