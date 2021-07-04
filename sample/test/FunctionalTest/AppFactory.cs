using System;
using System.Linq;
using Infrastructure.Persistence;
using MarkopTest.FunctionalTest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebAPI;
using Xunit.Abstractions;
using DatabaseInitializer = FunctionalTest.Persistence.DatabaseInitializer;

namespace FunctionalTest
{
    public class AppFactory : MarkopFunctionalTestFactory<Startup>
    {
        public AppFactory(ITestOutputHelper outputHelper, MarkopFunctionalTestOptions testOptions)
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