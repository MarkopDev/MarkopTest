using System.Threading.Tasks;
using Application.Features.News.Queries.GetNewsListFast;
using IntegrationTest.Handlers;
using Xunit;
using Xunit.Abstractions;

namespace LoadTest.Controller.News;

public class GetNewsListFastTests : AppFactory
{
    public GetNewsListFastTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Theory]
    [UserHandler]
    [InlineData(1, 100)]
    public async Task GetNewsListFast(int pageNumber, int pageSize)
    {
        var data = new GetNewsListFastQuery
        {
            PageSize = pageSize,
            PageNumber = pageNumber,
        };

        await PostJsonAsync(data);
    }
}