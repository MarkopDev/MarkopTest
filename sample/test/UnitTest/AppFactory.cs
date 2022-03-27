using System;
using System.Linq;
using Infrastructure.Persistence;
using MarkopTest.UnitTest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebAPI;
using Xunit.Abstractions;
using DatabaseInitializer = UnitTest.Persistence.DatabaseInitializer;

namespace UnitTest;

public class AppFactory : UnitTestFactory<Startup>
{
    public AppFactory(ITestOutputHelper outputHelper, UnitTestOptions testOptions = null)
        : base(outputHelper, testOptions ?? new UnitTestOptions {HostSeparation = true})
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

        services.AddDbContextPool<DatabaseContext>(options => { options.UseInMemoryDatabase("InMemoryDbForTesting"); });
    }
}