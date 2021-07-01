using System.Linq;
using System.Threading.Tasks;
using MarkopTest.IntegrationTest;
using UnitTest.ControllerTest.Admin.News;
using UnitTest.ControllerTest.News;
using UnitTest.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.ControllerTest
{
    public class ManageNews : AppFactory
    {
        public ManageNews(ITestOutputHelper outputHelper) : base(outputHelper, new MarkopIntegrationTestOptions())
        {
        }

        [Fact]
        public async Task ManageNewsTest()
        {
            var userClient = await Host.UserClient();
            var ownerClient = await Host.OwnerClient();

            var newsList = await new GetNewsListFastTest(OutputHelper, userClient).GetNewsListFast(1, 100, "");

            var currentNewsCount = newsList.Total;

            var createdNews = await new CreateNewsTest(OutputHelper, ownerClient).CreateNews("Test Title");

            newsList = await new GetNewsListFastTest(OutputHelper, userClient).GetNewsListFast(1, 100, "");

            // Should not visible to user until owner set isHidden to false
            Assert.Equal(newsList.Total, currentNewsCount);
            Assert.True(newsList.Data.All(n => n.Id != createdNews.News.Id));

            var editNews = await new EditNewsTest(OutputHelper, ownerClient).EditNews(createdNews.News.Id,
                "Edited Title", "Edited Content", "Edited Preview", false);

            newsList = await new GetNewsListFastTest(OutputHelper, userClient).GetNewsListFast(1, 100, "");

            Assert.Equal(newsList.Total, currentNewsCount + 1);
            Assert.Contains(newsList.Data, n => n.Id == createdNews.News.Id);

            currentNewsCount = newsList.Total;

            Assert.Equal("Edited Title", editNews.News.Title);
            Assert.Equal("Edited Content", editNews.News.Content);
            Assert.Equal("Edited Preview", editNews.News.Preview);

            await new DeleteNewsTest(OutputHelper, ownerClient).DeleteNews(createdNews.News.Id);

            newsList = await new GetNewsListFastTest(OutputHelper, userClient).GetNewsListFast(1, 100, "");

            Assert.Equal(newsList.Total, currentNewsCount - 1);
            Assert.True(newsList.Data.All(n => n.Id != createdNews.News.Id));
        }
    }
}