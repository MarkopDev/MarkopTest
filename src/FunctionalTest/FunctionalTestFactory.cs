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

namespace MarkopTest.FunctionalTest
{
    public abstract class FunctionalTestFactory<TStartup, TTestOptions>
        where TStartup : class
        where TTestOptions : FunctionalTestOptions, new()
    {
        private static IHost _host;
        protected readonly TTestOptions TestOptions;
        protected readonly ITestOutputHelper OutputHelper;

        protected FunctionalTestFactory(ITestOutputHelper outputHelper, TTestOptions testOptions = null)
        {
            OutputHelper = outputHelper;
            TestOptions = testOptions ?? new TTestOptions();

            var initial = new StackTrace().GetFrame(4)?.GetMethod()?.Name == "InvokeMethod" ||
                          new StackTrace().GetFrame(3)?.GetMethod()?.Name == "InvokeMethod";

            if (initial && (_host == null || TestOptions.HostSeparation))
                ConfigureWebHost();

            if (initial && _host != null)
                Initializer(_host.Services);
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
        protected FunctionalTestFactory(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected FunctionalTestFactory(ITestOutputHelper outputHelper, FunctionalTestOptions testOptions)
            : base(outputHelper, testOptions)
        {
        }
    }
}