using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.Models;
using Application.DTOs.News;
using Application.Features.Account.Commands.SignIn;
using Application.Features.Account.Commands.SignOut;
using Application.Features.Account.Commands.SignUp;
using Application.Features.News.Commands.CreateNews;
using Application.Features.News.Commands.DeleteNews;
using Application.Features.News.Commands.EditNews;
using Application.Features.News.Queries.GetNews;
using Application.Features.News.Queries.GetNewsListFast;
using Application.Features.News.Queries.GetNewsListSlow;
using IntegrationTest.Handlers;
using MarkopTest.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller.Admin;

[Endpoint("Admin/[controller]/[action]")]
public class NewsTests : AppFactory
{
    public NewsTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper, client)
    {
    }

    [Theory]
    [OwnerHandler]
    [InlineData("New Title")]
    [InlineData(null, ErrorCode.InvalidInput)]
    public async Task<CreateNewsViewModel> CreateNews(string title, ErrorCode? errorCode = null)
    {
        var data = new CreateNewsCommand
        {
            Title = title
        };

        var response = PostJson(data, new FetchOptions
        {
            ErrorCode = errorCode
        });

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<CreateNewsViewModel>()
            : null;
    }

    [Theory]
    [OwnerHandler]
    [InlineData(2)]
    [InlineData(-1, ErrorCode.InvalidInput)]
    public void DeleteNews(int newsId, ErrorCode? errorCode = null)
    {
        var data = new DeleteNewsCommand
        {
            NewsId = newsId
        };

        PostJson(data, new FetchOptions
        {
            ErrorCode = errorCode
        });
    }

    [Theory]
    [OwnerHandler]
    [InlineData(1, "Edited Title", "Edited Content", "Preview Edited", false)]
    [InlineData(1, "Edited Title", null, "Preview Edited", false, ErrorCode.InvalidInput)]
    [InlineData(1, "Edited Title", "Edited Content", null, false, ErrorCode.InvalidInput)]
    [InlineData(1, null, "Edited Content", "Preview Edited", false, ErrorCode.InvalidInput)]
    [InlineData(-1, "Edited Title", "Edited Content", "Preview Edited", false, ErrorCode.InvalidInput)]
    public async Task<EditNewsViewModel> EditNews(int newsId, string title, string content, string preview,
        bool isHidden, ErrorCode? errorCode = null)
    {
        var data = new EditNewsCommand
        {
            Title = title,
            NewsId = newsId,
            Content = content,
            Preview = preview,
            IsHidden = isHidden
        };

        var response = PostJson(data, new FetchOptions
        {
            ErrorCode = errorCode
        });

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<EditNewsViewModel>()
            : null;
    }
}