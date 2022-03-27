using System.Threading.Tasks;
using Infrastructure.Persistence;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTest.Tests;

public class DatabaseInitializerTests : AppFactory
{
    public DatabaseInitializerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task DatabaseInitializer()
    {
        var context = Services.GetService<DatabaseContext>();
        if (context != null)
            await context.Database.EnsureDeletedAsync();
        var initializer = new DatabaseInitializer(Services);
        await initializer.Initialize();
    }
}