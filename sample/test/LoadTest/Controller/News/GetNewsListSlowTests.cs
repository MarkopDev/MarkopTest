﻿using System.Net.Http;
using System.Threading.Tasks;
using Application.Features.News.Queries.GetNewsListSlow;
using IntegrationTest.Utilities;
using MarkopTest.LoadTest;
using Xunit;
using Xunit.Abstractions;

namespace LoadTest.Controller.News
{
    public class GetNewsListSlowTests : AppFactory
    {
        public GetNewsListSlowTests(ITestOutputHelper outputHelper, HttpClient client = null) : base(outputHelper,
            new MarkopLoadTestOptions {DefaultHttpClient = client})
        {
        }

        [Theory]
        [InlineData(1, 100)]
        public async Task GetNewsListSlow(int pageNumber, int pageSize)
        {
            Client ??= await Host.UserClient();
            
            var data = new GetNewsListSlowQuery
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
            };

            await Post(data, Client);
        }
    }
}