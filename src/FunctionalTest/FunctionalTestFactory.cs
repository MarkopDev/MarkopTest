using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace MarkopTest.FunctionalTest
{
    public abstract class FunctionalTestFactory<TStartup, TTestOptions>
        where TStartup : class
        where TTestOptions : FunctionalTestOptions, new()
    {
        private static IHost _host;
        private IHost _seperatedHost;
        private IServiceProvider _serviceProvider;
        protected readonly TTestOptions TestOptions;
        protected readonly ITestOutputHelper OutputHelper;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly ManualResetEventSlim _initializationTask = new ManualResetEventSlim(false);
        protected IServiceProvider Services => _serviceProvider.CreateScope().ServiceProvider;

        protected FunctionalTestFactory(ITestOutputHelper outputHelper, TTestOptions testOptions = null)
        {
            OutputHelper = outputHelper;
            TestOptions = testOptions ?? new TTestOptions();
        }

        private IHost Host => TestOptions.HostSeparation ? _seperatedHost : _host;

        protected HttpClient GetClient()
        {
            InitializeHost();

            _initializationTask.Wait(-1);

            return Host.GetTestClient();
        }

        private async void InitializeHost()
        {
            // Prevent call this method twice concurrently
            await _semaphore.WaitAsync();

            if (!_initializationTask.Wait(TimeSpan.Zero))
                await ConfigureWebHost();

            if (_initializationTask.Wait(TimeSpan.Zero) || Host == null)
                return;

            _serviceProvider = Host.Services;

            await Initializer(_serviceProvider);
            _initializationTask.Set();

            _semaphore.Release();
        }

        private async Task ConfigureWebHost()
        {
            if (!TestOptions.HostSeparation && _host != null)
                return;

            var hostBuilder = CreateHostBuilder();

            if (TestOptions.HostSeparation)
                _seperatedHost = await hostBuilder.StartAsync();
            else
                _host = await hostBuilder.StartAsync();
        }

        private IHostBuilder CreateHostBuilder() => new HostBuilder()
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

        protected abstract Task Initializer(IServiceProvider hostServices);
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