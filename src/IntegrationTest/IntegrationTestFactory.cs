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
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MarkopTest.Attributes;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Endpoint = MarkopTest.Attributes.Endpoint;
using HttpMethod = MarkopTest.Enums.HttpMethod;

namespace MarkopTest.IntegrationTest
{
    public abstract class IntegrationTestFactory<TStartup, TFetchOptions, TTestOptions>
        where TStartup : class
        where TFetchOptions : class
        where TTestOptions : IntegrationTestOptions, new()
    {
        private static IHost _host;

        private IHost _seperatedHost;

        // for passing the parameters in tests
        private readonly IServiceProvider _serviceProvider;

        protected readonly TTestOptions TestOptions;
        protected readonly HttpClient DefaultClient;
        protected readonly ITestOutputHelper OutputHelper;
        protected IServiceProvider Services => _serviceProvider.CreateScope().ServiceProvider;


        protected IntegrationTestFactory(ITestOutputHelper outputHelper, HttpClient defaultClient,
            TTestOptions testOptions = null)
        {
            OutputHelper = outputHelper;
            DefaultClient = defaultClient;
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

        protected virtual async Task PrintOutput(HttpResponseMessage response)
        {
            if (TestOptions.LogResponse)
                if (response.Content.Headers.Any(h =>
                        h.Key == "Content-Type" && h.Value.Any(v => v.Contains("application/json"))))
                    OutputHelper.WriteLine(await response.GetContent());
        }

        private string GetUrl()
        {
            var testMethod = new StackTrace().GetFrames().FirstOrDefault(frame =>
                    frame.GetMethod()?.GetCustomAttributes(typeof(Endpoint), true).Length > 0)?
                .GetMethod();

            var attributeObj = testMethod?.GetCustomAttributes(typeof(Endpoint), true).FirstOrDefault();

            if (attributeObj == null)
            {
                testMethod = new StackTrace().GetFrames().FirstOrDefault(frame =>
                        frame.GetMethod()?.DeclaringType?.GetCustomAttributes(typeof(Endpoint), true).Length > 0)
                    ?.GetMethod();
                attributeObj = testMethod?.DeclaringType?.GetCustomAttributes(typeof(Endpoint), true).FirstOrDefault();
            }

            if (!(attributeObj is Endpoint attribute))
                throw new NullReferenceException("Test method should have Endpoint attribute.");

            var template = attribute.Template;

            var controllerName = GetType().Name;
            if (controllerName.EndsWith("Tests"))
                controllerName = controllerName[..^5];
            else if (controllerName.EndsWith("Test"))
                controllerName = controllerName[..^4];
            else if (controllerName.EndsWith("Controller"))
                controllerName = controllerName[..^10];

            template = Regex.Replace(template, "\\[controller\\]", controllerName,
                RegexOptions.IgnoreCase);
            template = Regex.Replace(template, "\\[action\\]", testMethod.Name,
                RegexOptions.IgnoreCase);

            return GetUrl(template, controllerName, testMethod.Name);
        }

        protected HttpResponseMessage PostJson(dynamic data,
            TFetchOptions fetchOptions = null, TestHandlerOptions handlerOptions = null)
        {
            return RequestJson(data, HttpMethod.Post, fetchOptions, handlerOptions);
        }

        protected HttpResponseMessage Post(HttpContent content,
            TFetchOptions fetchOptions = null, TestHandlerOptions handlerOptions = null)
        {
            return Request(content, HttpMethod.Post, fetchOptions, handlerOptions);
        }

        protected HttpResponseMessage PutJson(dynamic data,
            TFetchOptions fetchOptions = null, TestHandlerOptions handlerOptions = null)
        {
            return RequestJson(data, HttpMethod.Put, fetchOptions, handlerOptions);
        }

        protected HttpResponseMessage Put(HttpContent content,
            TFetchOptions fetchOptions = null, TestHandlerOptions handlerOptions = null)
        {
            return Request(content, HttpMethod.Put, fetchOptions, handlerOptions);
        }

        protected HttpResponseMessage PatchJson(dynamic data,
            TFetchOptions fetchOptions = null, TestHandlerOptions handlerOptions = null)
        {
            return RequestJson(data, HttpMethod.Patch, fetchOptions, handlerOptions);
        }

        protected HttpResponseMessage Patch(HttpContent content,
            TFetchOptions fetchOptions = null, TestHandlerOptions handlerOptions = null)
        {
            return Request(content, HttpMethod.Patch, fetchOptions, handlerOptions);
        }

        private HttpResponseMessage RequestJson(dynamic data, HttpMethod method,
            TFetchOptions fetchOptions, TestHandlerOptions handlerOptions)
        {
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.Default, "application/json");

            return Request(content, method, fetchOptions, handlerOptions);
        }

        private HttpResponseMessage Request(HttpContent content, HttpMethod method,
            TFetchOptions fetchOptions, TestHandlerOptions handlerOptions)
        {
            var url = GetUrl();

            var testHandlerOptions = handlerOptions ?? new TestHandlerOptions
            {
                AfterRequest = true,
                BeforeRequest = true
            };

            return RequestAsync(url, content, method, fetchOptions, testHandlerOptions).Result;
        }


        private async Task<HttpResponseMessage> RequestAsync(string url, HttpContent content, HttpMethod method,
            TFetchOptions fetchOptions, TestHandlerOptions handlerOptions)
        {
            var client = GetClient();

            if (handlerOptions.BeforeRequest)
                await TestHandlerHelper.BeforeRequest(client, typeof(IntegrationTestFactory<>));

            var response = method switch
            {
                HttpMethod.Post => await client.PostAsync(url, content),
                HttpMethod.Put => await client.PutAsync(url, content),
                HttpMethod.Patch => await client.PatchAsync(url, content),
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
            };

            if (handlerOptions.AfterRequest)
                await TestHandlerHelper.AfterRequest(client, typeof(IntegrationTestFactory<>));

            await PrintOutput(response);

            Assert.True(await ValidateResponse(response, fetchOptions));

            return response;
        }

        protected async Task<HttpResponseMessage> GetAsync(dynamic data, TFetchOptions fetchOptions = null)
        {
            var client = GetClient();

            var uri = ""; // TODO

            var properties = data.GetType().GetProperties();
            foreach (var p in properties)
            {
                var value = p.GetValue(data, null);
                if (value != null && p.Name != null && p.Name.Length >= 1)
                {
                    uri += "?" + p.Name.Substring(0, 1).ToString().ToLower() + p.Name.Substring(1) + "=" +
                           (string)value;
                }
            }

            await TestHandlerHelper.BeforeRequest(client, typeof(IntegrationTestFactory<>));

            var response = await client.GetAsync(uri);

            await TestHandlerHelper.AfterRequest(client, typeof(IntegrationTestFactory<>));

            await PrintOutput(response);

            Assert.True(await ValidateResponse(response, fetchOptions));

            return response;
        }

        protected HttpClient GetClient()
        {
            return DefaultClient ?? Host.GetTestClient();
        }

        protected abstract string GetUrl(string url, string controllerName, string testMethodName);
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

    public class TestHandlerOptions
    {
        public bool BeforeRequest { set; get; } = true;
        public bool AfterRequest { set; get; } = true;
    }
}