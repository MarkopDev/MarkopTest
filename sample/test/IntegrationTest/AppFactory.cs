using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Application.Common.Enums;
using Infrastructure.Persistence;
using IntegrationTest.Utilities;
using MarkopTest.IntegrationTest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebAPI;
using Xunit;
using Xunit.Abstractions;
using DatabaseInitializer = IntegrationTest.Persistence.DatabaseInitializer;

namespace IntegrationTest
{
    public class AppFactory : MarkopIntegrationTestFactory<Startup, FetchOptions>
    {
        public AppFactory(ITestOutputHelper outputHelper, MarkopIntegrationTestOptions testOptions)
            : base(outputHelper, testOptions)
        {
        }

        protected override string GetUrl(string path, string actionName)
        {
            return APIs.V1 + path + actionName;
        }

        protected override void Initializer(IServiceProvider hostServices)
        {
            new DatabaseInitializer(hostServices).Initialize().GetAwaiter().GetResult();
        }

        protected override void ConfigureTestServices(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(d
                => d.ServiceType == typeof(DbContextOptions<DatabaseContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContextPool<DatabaseContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });
        }

        protected override async Task<bool> ValidateResponse(HttpResponseMessage httpResponseMessage,
            FetchOptions fetchOptions)
        {
            var responseHttpStatusCode = fetchOptions.HttpStatusCode;
            if (fetchOptions.ErrorCode != null)
                responseHttpStatusCode = HttpStatusCode.NotAcceptable;
            if (fetchOptions.ErrorCode == ErrorCode.Unauthorized)
                responseHttpStatusCode = HttpStatusCode.Unauthorized;

            Assert.Equal(responseHttpStatusCode, httpResponseMessage.StatusCode);

            if (responseHttpStatusCode is HttpStatusCode.OK or HttpStatusCode.NotAcceptable)
                return await httpResponseMessage.HasErrorCode(fetchOptions.ErrorCode);

            return true;
        }

        protected override async Task<HttpClient> GetDefaultClient()
        {
            if (Host == null)
                return null;
            return await Host.UserClient();
        }
    }
}