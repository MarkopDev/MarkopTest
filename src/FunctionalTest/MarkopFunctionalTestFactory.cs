using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace MarkopTest.FunctionalTest
{
    public abstract class MarkopFunctionalTestFactory<TStartup, TTestOptions>
        where TStartup : class
        where TTestOptions : MarkopFunctionalTestOptions
    {
        protected IHost Host;
        private readonly TTestOptions _testOptions;
        protected readonly ITestOutputHelper OutputHelper;

        protected MarkopFunctionalTestFactory(ITestOutputHelper outputHelper, TTestOptions testOptions)
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

    public abstract class MarkopFunctionalTestFactory<TStartup>
        : MarkopFunctionalTestFactory<TStartup, MarkopFunctionalTestOptions>
        where TStartup : class
    {
        protected MarkopFunctionalTestFactory(ITestOutputHelper outputHelper, MarkopFunctionalTestOptions testOptions)
            : base(outputHelper, testOptions)
        {
        }
    }
}