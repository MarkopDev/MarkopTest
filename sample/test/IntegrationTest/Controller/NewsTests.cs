using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.Models;
using Application.DTOs.News;
using Application.Features.News.Queries.GetNews;
using Application.Features.News.Queries.GetNewsListFast;
using Application.Features.News.Queries.GetNewsListSlow;
using IntegrationTest.Handlers;
using MarkopTest.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller;

[Endpoint("[controller]/[action]")]
public class NewsTests : AppFactory
{
    public NewsTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper, client)
    {
    }
    
    [MarkopTest.Attributes.Theory]
    [UserHandler]
    [InlineData(1, 5, "")]
    [InlineData(1, 5, "new")]
    [InlineData(1, -5, "", ErrorCode.InvalidInput)]
    [InlineData(-1, 5, "", ErrorCode.InvalidInput)]
    [InlineData(1, 5000, "", ErrorCode.InvalidInput)]
    public async Task<PaginationViewModel<NewsListItemDto>> GetNewsListFast(int pageNumber, int pageSize, string search,
        ErrorCode? errorCode = null)
    {
        var data = new GetNewsListFastQuery
        {
            Search = search,
            PageSize = pageSize,
            PageNumber = pageNumber,
        };

        var response =  PostJson(data, fetchOptions: new FetchOptions
        {
            ErrorCode = errorCode
        });

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<PaginationViewModel<NewsListItemDto>>()
            : null;
    }
    
    [MarkopTest.Attributes.Theory]
    [UserHandler]
    [InlineData(1, 5, "")]
    [InlineData(1, 5, "new")]
    [InlineData(1, -5, "", ErrorCode.InvalidInput)]
    [InlineData(-1, 5, "", ErrorCode.InvalidInput)]
    [InlineData(1, 5000, "", ErrorCode.InvalidInput)]
    public async Task<PaginationViewModel<NewsListItemDto>> GetNewsListSlow(int pageNumber, int pageSize, string search,
        ErrorCode? errorCode = null)
    {
        var data = new GetNewsListSlowQuery
        {
            Search = search,
            PageSize = pageSize,
            PageNumber = pageNumber,
        };

        var response =  PostJson(data, fetchOptions: new FetchOptions
        {
            ErrorCode = errorCode
        });

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<PaginationViewModel<NewsListItemDto>>()
            : null;
    }
    
    [MarkopTest.Attributes.Theory]
    [UserHandler]
    [InlineData(1)]
    [InlineData(-1, ErrorCode.InvalidInput)]
    public async Task<GetNewsViewModel> GetNews(int newsId, ErrorCode? errorCode = null)
    {
        var data = new GetNewsQuery
        {
            NewsId = newsId
        };

        var response =  PostJson(data, fetchOptions: new FetchOptions
        {
            ErrorCode = errorCode
        });

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<GetNewsViewModel>()
            : null;
    }
}