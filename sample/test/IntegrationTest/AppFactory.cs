using System;
using System.Linq;
using Infrastructure.Persistence;
using IntegrationTest.Persistence;
using MarkopTest.IntegrationTest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebAPI;
using Xunit.Abstractions;
using DatabaseInitializer = IntegrationTest.Persistence.DatabaseInitializer;

namespace IntegrationTest
{
    public class AppFactory : MarkopIntegrationTestFactory<Startup>
    {
        public AppFactory(ITestOutputHelper outputHelper, MarkopIntegrationTestOptions testOptions)
            : base(outputHelper, testOptions)
        {
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
    }
}