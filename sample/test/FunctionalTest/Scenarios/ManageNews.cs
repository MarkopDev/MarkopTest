using System.Linq;
using System.Threading.Tasks;
using IntegrationTest.Controller;
using IntegrationTest.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace FunctionalTest.Scenarios;

public class ManageNews : AppFactory
{
    public ManageNews(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [MarkopTest.Attributes.Fact]
    public async Task ManageNewsTest()
    {
        var userClient = await GetClient().User();
        var ownerClient = await GetClient().Owner();

        var newsList = await new NewsTests(OutputHelper, userClient).GetNewsListFast(1, 100, "");

        var currentNewsCount = newsList.Total;

        var createdNews =
            await new IntegrationTest.Controller.Admin.NewsTests(OutputHelper, ownerClient).CreateNews("Test Title");

        newsList = await new NewsTests(OutputHelper, userClient).GetNewsListFast(1, 100, "");

        // Should not visible to user until owner set isHidden to false
        Assert.Equal(newsList.Total, currentNewsCount);
        Assert.True(newsList.Data.All(n => n.Id != createdNews.News.Id));

        var editNews = await new IntegrationTest.Controller.Admin.NewsTests(OutputHelper, ownerClient).EditNews(
            createdNews.News.Id,
            "Edited Title", "Edited Content", "Edited Preview", false);

        newsList = await new NewsTests(OutputHelper, userClient).GetNewsListFast(1, 100, "");

        Assert.Equal(newsList.Total, currentNewsCount + 1);
        Assert.Contains(newsList.Data, n => n.Id == createdNews.News.Id);

        currentNewsCount = newsList.Total;

        Assert.Equal("Edited Title", editNews.News.Title);
        Assert.Equal("Edited Content", editNews.News.Content);
        Assert.Equal("Edited Preview", editNews.News.Preview);

        new IntegrationTest.Controller.Admin.NewsTests(OutputHelper, ownerClient).DeleteNews(createdNews.News.Id);

        newsList = await new NewsTests(OutputHelper, userClient).GetNewsListFast(1, 100, "");

        Assert.Equal(newsList.Total, currentNewsCount - 1);
        Assert.True(newsList.Data.All(n => n.Id != createdNews.News.Id));
    }
}