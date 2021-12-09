using System;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarkopTest.FunctionalTest;

public abstract class FunctionalTestFactory<TStartup, TTestOptions>
    where TStartup : class
    where TTestOptions : FunctionalTestOptions, new()
{
    private static IHost _host;
    private IHost _seperatedHost;
    protected readonly TTestOptions TestOptions;
    protected readonly ITestOutputHelper OutputHelper;

    protected FunctionalTestFactory(ITestOutputHelper outputHelper, TTestOptions testOptions = null)
    {
        OutputHelper = outputHelper;
        TestOptions = testOptions ?? new TTestOptions();

        var initial = new StackTrace().GetFrame(4)?.GetMethod()?.Name == "InvokeMethod" ||
                      new StackTrace().GetFrame(3)?.GetMethod()?.Name == "InvokeMethod";

        if (initial)
            ConfigureWebHost();

        if (initial && Host != null)
            Initializer(Host.Services);
    }

    private IHost Host => TestOptions.HostSeparation ? _seperatedHost : _host;

    private void ConfigureWebHost()
    {
        if (!TestOptions.HostSeparation && _host != null)
            return;

        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.UseStartup<TStartup>();
                webHost.UseConfiguration(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true)
                    .Build());

                webHost.ConfigureTestServices(ConfigureTestServices);
            });

        if (TestOptions.HostSeparation)
            _seperatedHost = hostBuilder.Start();
        else
            _host = hostBuilder.Start();
    }

    protected HttpClient GetClient()
    {
        return Host.GetTestClient();
    }

    protected abstract void Initializer(IServiceProvider hostServices);
    protected abstract void ConfigureTestServices(IServiceCollection services);
}

public abstract class FunctionalTestFactory<TStartup>
    : FunctionalTestFactory<TStartup, FunctionalTestOptions>
    where TStartup : class
{
    protected FunctionalTestFactory(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    protected FunctionalTestFactory(ITestOutputHelper outputHelper, FunctionalTestOptions testOptions)
        : base(outputHelper, testOptions)
    {
    }
}