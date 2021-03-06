using WebAPI;
using System;
using System.Linq;
using Xunit.Abstractions;
using MarkopTest.FunctionalTest;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DatabaseInitializer = FunctionalTest.Persistence.DatabaseInitializer;

namespace FunctionalTest;

public class AppFactory : FunctionalTestFactory<Startup>
{
    public AppFactory(ITestOutputHelper outputHelper) : base(outputHelper, new FunctionalTestOptions
    {
        HostSeparation = true
    })
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

        // Use this trick to have different database in host separation
        var dbName = "InMemoryDbForTesting" + Guid.NewGuid();
        services.AddDbContextPool<DatabaseContext>(options =>
        {
            options.UseInMemoryDatabase(dbName);
        });
    }
}