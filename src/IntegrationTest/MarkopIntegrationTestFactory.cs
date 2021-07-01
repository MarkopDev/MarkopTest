using System;
using System.IO;
using System.Diagnostics;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarkopTest.IntegrationTest
{
    public abstract class MarkopIntegrationTestFactory<TStartup, TTestOptions>
        where TStartup : class
        where TTestOptions : MarkopIntegrationTestOptions
    {
        protected IHost Host;
        private readonly TTestOptions _testOptions;
        protected readonly ITestOutputHelper OutputHelper;

        protected MarkopIntegrationTestFactory(ITestOutputHelper outputHelper, TTestOptions testOptions)
        {
            _testOptions = testOptions;
            OutputHelper = outputHelper;

            var initial = new StackTrace().GetFrame(4)?.GetMethod()?.Name == "InvokeMethod" ||
                          new StackTrace().GetFrame(3)?.GetMethod()?.Name == "InvokeMethod";

            if (initial)
            {
                ConfigureWebHost();
                Initializer(Host.Services);
            }
        }

        private void ConfigureWebHost()
        {
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

            Host = hostBuilder.Start();
        }

        protected abstract void Initializer(IServiceProvider hostServices);
        protected abstract void ConfigureTestServices(IServiceCollection services);
    }

    public abstract class MarkopIntegrationTestFactory<TStartup>
        : MarkopIntegrationTestFactory<TStartup, MarkopIntegrationTestOptions>
        where TStartup : class
    {
        protected MarkopIntegrationTestFactory(ITestOutputHelper outputHelper, MarkopIntegrationTestOptions testOptions)
            : base(outputHelper, testOptions)
        {
        }
    }
}