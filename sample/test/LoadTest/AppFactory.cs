using System;
using System.Linq;
using Infrastructure.Persistence;
using MarkopTest.LoadTest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebAPI;
using Xunit.Abstractions;
using DatabaseInitializer = LoadTest.Persistence.DatabaseInitializer;

namespace LoadTest
{
    public class AppFactory : MarkopLoadTestFactory<Startup>
    {
        public AppFactory(ITestOutputHelper outputHelper, MarkopLoadTestOptions loadTestOptions) : base(outputHelper,
            loadTestOptions)
        {
            loadTestOptions.ChartColor = "#427bcb";
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

            services.AddDbContext<DatabaseContext>(options =>
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ==
                    Environments.Development)
                    options.UseNpgsql(services.BuildServiceProvider().GetService<IConfiguration>()
                        .GetConnectionString("DBConnectionTestPostgreSQL"));
                else
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
            }, ServiceLifetime.Transient);
        }
    }
}