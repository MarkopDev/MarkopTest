﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using Xunit.Abstractions;

namespace MarkopTest.UnitTest
{
    public abstract class MarkopUnitTestFactory<TStartup, TFetchOptions, TTestOptions>
        where TStartup : class
        where TFetchOptions : class
        where TTestOptions : MarkopUnitTestOptions
    {
        protected IHost Host;
        public readonly string Uri;
        protected HttpClient Client;
        private readonly TTestOptions _testOptions;
        private readonly ITestOutputHelper _outputHelper;

        protected MarkopUnitTestFactory(ITestOutputHelper outputHelper, TTestOptions testOptions)
        {
            _testOptions = testOptions;
            _outputHelper = outputHelper;

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
            if (actionName.EndsWith("Test"))
                actionName = actionName.Substring(0, actionName.Length - 4);

            var path = "";
            var nameSpace = GetType().Namespace;

            while (!(nameSpace?.EndsWith("ControllerTest") ?? true))
            {
                var controller = nameSpace.Split(".").Last();

                nameSpace = nameSpace[..(nameSpace.Length - controller.Length - 1)];

                if (controller.EndsWith("Test"))
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

        protected async Task<HttpResponseMessage> Fetch(dynamic data, HttpClient client = null,
            TFetchOptions fetchOptions = null)
        {
            client ??= await GetDefaultClient() ?? Client;

            HttpResponseMessage response =
                await client.PostAsync(Uri, JsonContent.Create(data));

            if (_testOptions.LogResponse)
                _outputHelper.WriteLine(await response.GetContent());

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

    public abstract class MarkopUnitTestFactory<TStartup, TFetchOption>
        : MarkopUnitTestFactory<TStartup, TFetchOption, MarkopUnitTestOptions>
        where TStartup : class
        where TFetchOption : class
    {
        protected MarkopUnitTestFactory(ITestOutputHelper outputHelper, MarkopUnitTestOptions testOptions)
            : base(outputHelper, testOptions)
        {
        }
    }

    public abstract class MarkopUnitTestFactory<TStartup>
        : MarkopUnitTestFactory<TStartup, dynamic, MarkopUnitTestOptions>
        where TStartup : class
    {
        protected MarkopUnitTestFactory(ITestOutputHelper outputHelper, MarkopUnitTestOptions testOptions)
            : base(outputHelper, testOptions)
        {
        }
    }
}