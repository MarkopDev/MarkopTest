using System;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebAPI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await ServiceProvider(host);
        await host.RunAsync();
    }

    private static async Task ServiceProvider(IHost host)
    {
        using var scope = host.Services.CreateScope();
        try
        {
            await new DatabaseInitializer(scope.ServiceProvider).Initialize();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://0.0.0.0:5000").UseStartup<Startup>();
            });
}