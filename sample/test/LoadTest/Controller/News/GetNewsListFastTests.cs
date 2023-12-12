using Application.Features.News.Queries.GetNewsListFast;
using IntegrationTest.Handlers;
using MarkopTest.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace LoadTest.Controller.News;

public class GetNewsListFastTests : AppFactory
{
    public GetNewsListFastTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [MarkopTest.Attributes.Theory]
    [UserHandler]
    [InlineData(1, 100)]
    [Endpoint("News/GetNewsListFast")]
    public void GetNewsListFast(int pageNumber, int pageSize)
    {
        var data = new GetNewsListFastQuery
        {
            PageSize = pageSize,
            PageNumber = pageNumber,
        };

        PostJson(data);
    }
}