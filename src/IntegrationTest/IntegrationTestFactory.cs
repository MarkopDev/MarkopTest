using Xunit;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Xunit.Abstractions;
using MarkopTest.Handler;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarkopTest.IntegrationTest
{
    public abstract class IntegrationTestFactory<TStartup, TFetchOptions, TTestOptions>
        where TStartup : class
        where TFetchOptions : class
        where TTestOptions : IntegrationTestOptions, new()
    {
        private static IHost _host;
        public readonly string Uri;
        protected readonly TTestOptions TestOptions;
        protected readonly HttpClient DefaultClient;
        protected readonly ITestOutputHelper OutputHelper;

        protected IntegrationTestFactory(ITestOutputHelper outputHelper, HttpClient defaultClient,
            TTestOptions testOptions = null)
        {
            OutputHelper = outputHelper;
            DefaultClient = defaultClient;
            TestOptions = testOptions ?? new TTestOptions();

            var initial = new StackTrace().GetFrame(4)?.GetMethod()?.Name == "InvokeMethod" ||
                          new StackTrace().GetFrame(3)?.GetMethod()?.Name == "InvokeMethod";

            if (initial && (_host == null || TestOptions.HostSeparation))
                ConfigureWebHost();

            if (initial && _host != null)
                Initializer(_host.Services);

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

            _host = hostBuilder.Start();
        }

        protected virtual async Task PrintOutput(HttpResponseMessage response)
        {
            if (TestOptions.LogResponse)
                if (response.Content.Headers.Any(h =>
                    h.Key == "Content-Type" && h.Value.Any(v => v.Contains("application/json"))))
                    OutputHelper.WriteLine(await response.GetContent());
        }

        protected async Task<HttpResponseMessage> PostJsonAsync(dynamic data,
            TFetchOptions fetchOptions = null)
        {
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.Default, "application/json");
            return await PostAsync(content, fetchOptions);
        }

        protected async Task<HttpResponseMessage> PostAsync(HttpContent content, TFetchOptions fetchOptions = null)
        {
            var client = GetClient();

            await TestHandlerHelper.BeforeTest(client, typeof(IntegrationTestFactory<>));

            var response = await client.PostAsync(Uri, content);

            await TestHandlerHelper.AfterTest(client, typeof(IntegrationTestFactory<>));

            await PrintOutput(response);

            Assert.True(await ValidateResponse(response, fetchOptions));

            return response;
        }

        protected async Task<HttpResponseMessage> GetAsync(dynamic data, TFetchOptions fetchOptions = null)
        {
            var client = GetClient();

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

            await TestHandlerHelper.BeforeTest(client, typeof(IntegrationTestFactory<>));

            var response = await client.GetAsync(uri);

            await TestHandlerHelper.AfterTest(client, typeof(IntegrationTestFactory<>));

            await PrintOutput(response);

            Assert.True(await ValidateResponse(response, fetchOptions));

            return response;
        }

        protected HttpClient GetClient()
        {
            return DefaultClient ?? _host.GetTestClient();
        }

        protected abstract string GetUrl(string path, string actionName);
        protected abstract void Initializer(IServiceProvider hostServices);
        protected abstract void ConfigureTestServices(IServiceCollection services);

        protected abstract Task<bool> ValidateResponse(HttpResponseMessage httpResponseMessage,
            TFetchOptions fetchOptions);
    }

    public abstract class IntegrationTestFactory<TStartup, TFetchOption>
        : IntegrationTestFactory<TStartup, TFetchOption, IntegrationTestOptions>
        where TStartup : class
        where TFetchOption : class
    {
        protected IntegrationTestFactory(ITestOutputHelper outputHelper, HttpClient defaultClient)
            : base(outputHelper, defaultClient)
        {
        }

        protected IntegrationTestFactory(ITestOutputHelper outputHelper, HttpClient defaultClient,
            IntegrationTestOptions testOptions)
            : base(outputHelper, defaultClient, testOptions)
        {
        }
    }

    public abstract class IntegrationTestFactory<TStartup>
        : IntegrationTestFactory<TStartup, dynamic, IntegrationTestOptions>
        where TStartup : class
    {
        protected IntegrationTestFactory(ITestOutputHelper outputHelper, HttpClient defaultClient)
            : base(outputHelper, defaultClient)
        {
        }

        protected IntegrationTestFactory(ITestOutputHelper outputHelper, HttpClient defaultClient,
            IntegrationTestOptions testOptions)
            : base(outputHelper, defaultClient, testOptions)
        {
        }
    }
}