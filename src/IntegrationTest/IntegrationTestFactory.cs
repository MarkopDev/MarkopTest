using Xunit;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Text.Json;
using Xunit.Abstractions;
using MarkopTest.Handler;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using HttpMethod = MarkopTest.Enums.HttpMethod;
using Microsoft.Extensions.DependencyInjection;
using Endpoint = MarkopTest.Attributes.Endpoint;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace MarkopTest.IntegrationTest
{
    public abstract class IntegrationTestFactory<TStartup, TFetchOptions, TTestOptions>
        where TStartup : class
        where TFetchOptions : class
        where TTestOptions : IntegrationTestOptions, new()
    {
        private static IHost _host;
        private IHost _seperatedHost;
        private IServiceProvider _serviceProvider;

        protected readonly TTestOptions TestOptions;
        protected readonly HttpClient DefaultClient;
        protected readonly ITestOutputHelper OutputHelper;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly ManualResetEventSlim _initializationTask = new ManualResetEventSlim(false);
        protected IServiceProvider Services => _serviceProvider.CreateScope().ServiceProvider;

        protected IntegrationTestFactory(ITestOutputHelper outputHelper, HttpClient defaultClient,
            TTestOptions testOptions = null)
        {
            OutputHelper = outputHelper;
            DefaultClient = defaultClient;
            TestOptions = testOptions ?? new TTestOptions();
        }

        private IHost Host => TestOptions.HostSeparation ? _seperatedHost : _host;

        private async Task InitializeHost()
        {
            // Prevent call this method twice concurrently
            await _semaphore.WaitAsync();

            if (!_initializationTask.Wait(TimeSpan.Zero))
                await ConfigureWebHost();

            if (_initializationTask.Wait(TimeSpan.Zero) || Host == null)
                return;

            _serviceProvider = Host.Services;
            await Initializer(Host.Services);

            _initializationTask.Set();

            _semaphore.Release();
        }

        private async Task ConfigureWebHost()
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
                _seperatedHost = await hostBuilder.StartAsync();
            else
                _host = await hostBuilder.StartAsync();
        }

        protected virtual async Task PrintOutput(HttpResponseMessage response)
        {
            if (TestOptions.LogResponse)
                if (response.Content.Headers.Any(h =>
                        h.Key == "Content-Type" && h.Value.Any(v => v.Contains("application/json"))))
                    OutputHelper.WriteLine(await response.GetContent());
            // TODO Handle response output
        }

        protected HttpClient GetClient()
        {
            if (DefaultClient != null)
                return DefaultClient;

            InitializeHost();

            _initializationTask.Wait(-1);

            return Host.GetTestClient();
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

        #region Json Methods

        protected HttpResponseMessage PostJson(dynamic data,
            TFetchOptions fetchOptions = null, TestHandlerOptions handlerOptions = null)
        {
            return RequestJson(data, HttpMethod.Post, fetchOptions, handlerOptions);
        }

        protected HttpResponseMessage PutJson(dynamic data,
            TFetchOptions fetchOptions = null, TestHandlerOptions handlerOptions = null)
        {
            return RequestJson(data, HttpMethod.Put, fetchOptions, handlerOptions);
        }

        protected HttpResponseMessage PatchJson(dynamic data,
            TFetchOptions fetchOptions = null, TestHandlerOptions handlerOptions = null)
        {
            return RequestJson(data, HttpMethod.Patch, fetchOptions, handlerOptions);
        }

        #endregion

        #region Non-Json Methods

        protected HttpResponseMessage Get(dynamic data, TFetchOptions fetchOptions = null,
            TestHandlerOptions handlerOptions = null)
        {
            var url = GetUrl();

            var testHandlerOptions = handlerOptions ?? new TestHandlerOptions
            {
                AfterRequest = true,
                BeforeRequest = true
            };

            var client = GetClient();

            var handler = TestHandlerHelper.GetTestHandler(typeof(IntegrationTestFactory<>));

            HttpResponseMessage response = null;
            var thread = new Thread(() =>
            {
                if (testHandlerOptions.BeforeRequest && handler != null)
                    handler.BeforeRequest(client).GetAwaiter().GetResult();

                response = GetAsync(url, client, data, fetchOptions).Result;

                if (testHandlerOptions.AfterRequest && handler != null)
                    handler.AfterRequest(client).GetAwaiter().GetResult();
            });
            thread.Start();
            thread.Join();

            return response;
        }

        private async Task<HttpResponseMessage> GetAsync(string url, HttpClient client, dynamic data,
            TFetchOptions fetchOptions)
        {
            var properties = data.GetType().GetProperties();
            foreach (var p in properties)
            {
                var value = p.GetValue(data, null);
                if (value != null && p.Name != null && p.Name.Length >= 1)
                {
                    url += "?" + p.Name.Substring(0, 1).ToString().ToLower() + p.Name.Substring(1) + "=" +
                           (string)value;
                }
            }

            var response = await client.GetAsync(url);

            await PrintOutput(response);

            Assert.True(await ValidateResponse(response, fetchOptions));

            return response;
        }

        protected HttpResponseMessage Post(HttpContent content,
            TFetchOptions fetchOptions = null, TestHandlerOptions handlerOptions = null)
        {
            return Request(content, HttpMethod.Post, fetchOptions, handlerOptions);
        }

        protected HttpResponseMessage Put(HttpContent content,
            TFetchOptions fetchOptions = null, TestHandlerOptions handlerOptions = null)
        {
            return Request(content, HttpMethod.Put, fetchOptions, handlerOptions);
        }

        protected HttpResponseMessage Patch(HttpContent content,
            TFetchOptions fetchOptions = null, TestHandlerOptions handlerOptions = null)
        {
            return Request(content, HttpMethod.Patch, fetchOptions, handlerOptions);
        }

        #endregion

        #region Request Methods

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

            var client = GetClient();

            var handler = TestHandlerHelper.GetTestHandler(typeof(IntegrationTestFactory<>));

            Exception exception = null;
            HttpResponseMessage response = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (testHandlerOptions.BeforeRequest && handler != null)
                        handler.BeforeRequest(client).GetAwaiter().GetResult();

                    response = RequestAsync(url, client, content, method, fetchOptions).Result;

                    if (testHandlerOptions.AfterRequest && handler != null)
                        handler.AfterRequest(client).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    exception = e;
                }
            });
            thread.Start();
            thread.Join();

            if (exception != null)
                throw exception;

            return response;
        }

        private async Task<HttpResponseMessage> RequestAsync(string url, HttpClient client, HttpContent content,
            HttpMethod method, TFetchOptions fetchOptions)
        {
            var response = method switch
            {
                // TODO add delete method
                HttpMethod.Put => await client.PutAsync(url, content),
                HttpMethod.Post => await client.PostAsync(url, content),
                HttpMethod.Patch => await client.PatchAsync(url, content),
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
            };


            await PrintOutput(response);
            Assert.True(await ValidateResponse(response, fetchOptions));

            return response;
        }

        #endregion

        #region Abstract Methods

        protected abstract string GetUrl(string url, string controllerName, string testMethodName);
        protected abstract Task Initializer(IServiceProvider hostServices);
        protected abstract void ConfigureTestServices(IServiceCollection services);

        protected abstract Task<bool> ValidateResponse(HttpResponseMessage httpResponseMessage,
            TFetchOptions fetchOptions);

        #endregion
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