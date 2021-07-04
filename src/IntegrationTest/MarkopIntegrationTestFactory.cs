using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace MarkopTest.IntegrationTest
{
    public abstract class MarkopIntegrationTestFactory<TStartup, TFetchOptions, TTestOptions>
        where TStartup : class
        where TFetchOptions : class
        where TTestOptions : MarkopIntegrationTestOptions
    {
        protected IHost Host;
        public readonly string Uri;
        protected HttpClient Client;
        private readonly TTestOptions _testOptions;
        protected readonly ITestOutputHelper OutputHelper;

        protected MarkopIntegrationTestFactory(ITestOutputHelper outputHelper, TTestOptions testOptions)
        {
            _testOptions = testOptions;
            OutputHelper = outputHelper;

            Client = testOptions.DefaultHttpClient;

            var initial = new StackTrace().GetFrame(4)?.GetMethod()?.Name == "InvokeMethod" ||
                          new StackTrace().GetFrame(3)?.GetMethod()?.Name == "InvokeMethod";

            if (initial)
            {
                ConfigureWebHost();
                Initializer(Host.Services);
            }

            #region AnalizeNamespace

            var actionName = GetType().Name;
            if (actionName.EndsWith("Tests"))
                actionName = actionName[..^5];
            else if (actionName.EndsWith("Test"))
                actionName = actionName[..^4];

            var path = "";
            var nameSpace = GetType().Namespace;

            while (!(nameSpace?.EndsWith("Controller") ?? true))
            {
                var controller = nameSpace.Split(".").Last();

                nameSpace = nameSpace[..(nameSpace.Length - controller.Length - 1)];

                if (controller.EndsWith("Tests"))
                    controller = controller[..^5];
                else if (controller.EndsWith("Test"))
                    controller = controller[..^4];

                path = controller + "/" + path;
            }

            #endregion

            Uri = GetUrl(path, actionName);
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

        protected async Task<HttpResponseMessage> Post(dynamic data, HttpClient client = null,
            TFetchOptions fetchOptions = null)
        {
            client ??= await GetDefaultClient() ?? Client;

            HttpResponseMessage response =
                await client.PostAsync(Uri, new StringContent(JsonSerializer.Serialize(data), Encoding.Default, "application/json"));

            if (_testOptions.LogResponse)
                OutputHelper.WriteLine(await response.GetContent());

            Assert.True(await ValidateResponse(response, fetchOptions));

            return response;
        }

        protected async Task<HttpResponseMessage> Get(dynamic data, HttpClient client = null,
            TFetchOptions fetchOptions = null)
        {
            client ??= await GetDefaultClient() ?? Client;

            var uri = Uri;

            var properties = data.GetType().GetProperties();
            foreach (var p in properties)
            {
                var value = p.GetValue(data, null);
                if (value != null && p.Name != null && p.Name.Length >= 1)
                {
                    uri += "?" + p.Name.Substring(0, 1).ToString().ToLower() + p.Name.Substring(1) + "=" +
                           (string) value;
                }
            }
            
            var response = await client.GetAsync(uri);

            if (_testOptions.LogResponse)
                OutputHelper.WriteLine(await response.GetContent());

            Assert.True(await ValidateResponse(response, fetchOptions));

            return response;
        }

        protected virtual async Task<HttpClient> GetDefaultClient()
        {
            return _testOptions.DefaultHttpClient;
        }

        protected abstract string GetUrl(string path, string actionName);
        protected abstract void Initializer(IServiceProvider hostServices);
        protected abstract void ConfigureTestServices(IServiceCollection services);

        protected abstract Task<bool> ValidateResponse(HttpResponseMessage httpResponseMessage,
            TFetchOptions fetchOptions);
    }

    public abstract class MarkopIntegrationTestFactory<TStartup, TFetchOption>
        : MarkopIntegrationTestFactory<TStartup, TFetchOption, MarkopIntegrationTestOptions>
        where TStartup : class
        where TFetchOption : class
    {
        protected MarkopIntegrationTestFactory(ITestOutputHelper outputHelper, MarkopIntegrationTestOptions testOptions)
            : base(outputHelper, testOptions)
        {
        }
    }

    public abstract class MarkopIntegrationTestFactory<TStartup>
        : MarkopIntegrationTestFactory<TStartup, dynamic, MarkopIntegrationTestOptions>
        where TStartup : class
    {
        protected MarkopIntegrationTestFactory(ITestOutputHelper outputHelper, MarkopIntegrationTestOptions testOptions)
            : base(outputHelper, testOptions)
        {
        }
    }
}