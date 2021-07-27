using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace MarkopTest.FunctionalTest
{
    public abstract class FunctionalTestFactory<TStartup, TTestOptions>
        where TStartup : class
        where TTestOptions : FunctionalTestOptions
    {
        private static IHost _host;
        private readonly TTestOptions _testOptions;
        protected readonly ITestOutputHelper OutputHelper;

        protected FunctionalTestFactory(ITestOutputHelper outputHelper, TTestOptions testOptions)
        {
            _testOptions = testOptions;
            OutputHelper = outputHelper;

            var initial = new StackTrace().GetFrame(4)?.GetMethod()?.Name == "InvokeMethod" ||
                          new StackTrace().GetFrame(3)?.GetMethod()?.Name == "InvokeMethod";

            if (initial && _host == null)
            {
                ConfigureWebHost();
                if (_host != null)
                    Initializer(_host.Services);
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

            _host = hostBuilder.Start();
        }

        protected HttpClient GetClient()
        {
            return _host.GetTestClient();
        }

        protected abstract void Initializer(IServiceProvider hostServices);
        protected abstract void ConfigureTestServices(IServiceCollection services);
    }

    public abstract class FunctionalTestFactory<TStartup>
        : FunctionalTestFactory<TStartup, FunctionalTestOptions>
        where TStartup : class
    {
        protected FunctionalTestFactory(ITestOutputHelper outputHelper, FunctionalTestOptions testOptions)
            : base(outputHelper, testOptions)
        {
        }
    }
}