﻿using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Common.Enums;
using Application.Common.Models;
using Application.DTOs.News;
using Application.Features.News.Queries.GetNewsListFast;
using MarkopTest.IntegrationTest;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Controller.News
{
    public class GetNewsListFastTests : AppFactory
    {
        public GetNewsListFastTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopIntegrationTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
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

            var response = await PostJsonAsync(data, Client, new FetchOptions
            {
                ErrorCode = errorCode
            });

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<PaginationViewModel<NewsListItemDto>>()
                : null;
        }
    }
}