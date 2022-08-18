using WebAPI;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using MarkopTest.FunctionalTest;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DatabaseInitializer = FunctionalTest.Persistence.DatabaseInitializer;

namespace FunctionalTest;

public class AppFactory : FunctionalTestFactory<Startup>
{
    public AppFactory(ITestOutputHelper outputHelper) : base(outputHelper, new FunctionalTestOptions())
    {
    }

    protected override async Task Initializer(IServiceProvider hostServices)
    {
        await new DatabaseInitializer(hostServices).Initialize();
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d
            => d.ServiceType == typeof(DbContextOptions<DatabaseContext>));

        if (descriptor != null)
            services.Remove(descriptor);

        // Use this trick to have different database in host separation
        var dbName = "InMemoryDbForTesting" + Guid.NewGuid();
        services.AddDbContextPool<DatabaseContext>(options => options.UseInMemoryDatabase(dbName));
    }
}