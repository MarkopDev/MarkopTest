using Application.Features.News.Queries.GetNewsListSlow;
using IntegrationTest.Handlers;
using MarkopTest.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace LoadTest.Controller.News;

public class GetNewsListSlowTests : AppFactory
{
    public GetNewsListSlowTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Theory]
    [UserHandler]
    [InlineData(1, 100)]
    [Endpoint("News/GetNewsList")]
    public void GetNewsListSlow(int pageNumber, int pageSize)
    {
        var data = new GetNewsListSlowQuery
        {
            PageSize = pageSize,
            PageNumber = pageNumber,
        };

        PostJson(data);
    }
}