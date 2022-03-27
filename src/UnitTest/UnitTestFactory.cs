using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace MarkopTest.UnitTest
{
    public abstract class UnitTestFactory<TStartup, TTestOptions>
        where TStartup : class where TTestOptions : UnitTestOptions, new()
    {
        private static IHost _host;
        private IHost _seperatedHost;
        // for passing the parameters in tests
        private readonly IServiceProvider _serviceProvider;
        
        protected readonly TTestOptions TestOptions;
        protected readonly ITestOutputHelper OutputHelper;
        protected IServiceProvider Services => _serviceProvider.CreateScope().ServiceProvider;

        protected UnitTestFactory(ITestOutputHelper outputHelper, TTestOptions testOptions = null)
        {
            OutputHelper = outputHelper;
            TestOptions = testOptions ?? new TTestOptions();

            var initial = new StackTrace().GetFrame(4)?.GetMethod()?.Name == "InvokeMethod" ||
                          new StackTrace().GetFrame(3)?.GetMethod()?.Name == "InvokeMethod";

            if (initial)
                ConfigureWebHost();

            if (initial && Host != null)
            {
                Initializer(Host.Services);
                _serviceProvider = Host.Services;
            }
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

        protected abstract void Initializer(IServiceProvider hostServices);
        protected abstract void ConfigureTestServices(IServiceCollection services);
    }

    public abstract class UnitTestFactory<TStartup> : UnitTestFactory<TStartup, UnitTestOptions> where TStartup : class
    {
        protected UnitTestFactory(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected UnitTestFactory(ITestOutputHelper outputHelper, UnitTestOptions testOptions) : base(outputHelper,
            testOptions)
        {
        }
    }
}