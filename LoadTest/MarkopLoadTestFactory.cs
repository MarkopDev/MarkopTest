using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MarkopTest.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RazorLight;
using Xunit.Abstractions;

namespace MarkopTest.LoadTest
{
    public abstract class MarkopLoadTestFactory<TStartup, TFetchOptions, TTestOptions>
        where TStartup : class
        where TFetchOptions : class
        where TTestOptions : MarkopLoadTestOptions
    {
        protected IHost Host;
        protected HttpClient Client;
        private readonly string _uri;
        private readonly TTestOptions _testOptions;
        private readonly ITestOutputHelper _outputHelper;

        protected MarkopLoadTestFactory(ITestOutputHelper outputHelper, TTestOptions testOptions)
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

            _uri = GetUrl(path, actionName);
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

        protected async Task Fetch(dynamic data, HttpClient client = null,
            TFetchOptions fetchOptions = null)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != Environments.Development)
                return;

            client ??= await GetDefaultClient() ?? Client;

            var syncTimes = new ConcurrentDictionary<int, long>();
            var asyncTimes = new ConcurrentDictionary<int, long>();
            var tasks = new List<Task>();

            for (var i = 0; i < 50; i++)
            {
                var sw = Stopwatch.StartNew();

                await client.PostAsync(_uri, JsonContent.Create(data));

                sw.Stop();
                var milliseconds = sw.ElapsedMilliseconds;
                while (!syncTimes.TryAdd(i, milliseconds))
                {
                    Thread.Sleep(1000);
                }
            }

            for (var i = 0; i < 1000; i++)
            {
                var i1 = i;
                tasks.Add(Task.Run(async () =>
                {
                    var sw = Stopwatch.StartNew();

                    await client.PostAsync(_uri, JsonContent.Create(data));

                    sw.Stop();
                    var milliseconds = sw.ElapsedMilliseconds;
                    while (!asyncTimes.TryAdd(i1, milliseconds))
                    {
                        Thread.Sleep(1000);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            var syncAverage = syncTimes.Select(t => t.Value).Sum() / syncTimes.Count;
            var asyncAverage = asyncTimes.Select(t => t.Value).Sum() / asyncTimes.Count;
            _outputHelper.WriteLine(new string('-', 20));
            _outputHelper.WriteLine($"Sync Min: {syncTimes.Min(t => t.Value) / 1000.0}Sec");
            _outputHelper.WriteLine($"Sync Average: {syncAverage / 1000.0}Sec");
            _outputHelper.WriteLine($"Sync Max: {syncTimes.Max(t => t.Value) / 1000.0}Sec");
            _outputHelper.WriteLine($"Async Min: {asyncTimes.Min(t => t.Value) / 1000.0}Sec");
            _outputHelper.WriteLine($"Async Average: {asyncAverage / 1000.0}Sec");
            _outputHelper.WriteLine($"Async Max: {asyncTimes.Max(t => t.Value) / 1000.0}Sec");

            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Environment.CurrentDirectory)
                .UseMemoryCachingProvider()
                .Build();

            var syncTimesArray = syncTimes.Values;
            var asyncTimesArray = asyncTimes.Values;

            var syncTimesIterationArray = syncTimes.Values.Select((l, i) => new[] {i, l});
            var asyncTimesIterationArray = asyncTimes.Values.Select((l, i) => new[] {i, l});

            var syncTimesDistributionArray = syncTimesArray.GroupBy(value => value)
                .Select(group => { return new[] {group.Key, group.Count()}; })
                .OrderBy(x => x[1]);
            var asyncTimesDistributionArray = asyncTimesArray.GroupBy(value => value)
                .Select(group => { return new[] {group.Key, group.Count()}; })
                .OrderBy(x => x[1]);

            var model = new ExportResultModel
            {
                SyncTimesIterationsJsArray = JsonSerializer.Serialize(syncTimesIterationArray),
                AsyncTimesIterationsJsArray = JsonSerializer.Serialize(asyncTimesIterationArray),
                SyncTimesDistributionJsArray = JsonSerializer.Serialize(syncTimesDistributionArray),
                AsyncTimesDistributionJsArray = JsonSerializer.Serialize(asyncTimesDistributionArray),
            };

            var result = await engine.CompileRenderAsync("Template/Result.cshtml", model);

            if (Directory.Exists("LoadTestResult"))
                Directory.Delete("LoadTestResult", true);

            Directory.CreateDirectory("LoadTestResult");
            Directory.CreateDirectory("LoadTestResult/scripts");
            foreach (var script in Directory.GetFiles("Template/scripts"))
                File.Copy(script, Path.Combine("LoadTestResult/scripts", Path.GetFileName(script)));

            await using var file = File.Create("LoadTestResult/Result.html");
            file.Write(Encoding.UTF8.GetBytes(result));

            if (_testOptions.OpenResultAfterFinished)
            {
                Process.Start(@"cmd.exe", @"/c " + Path.GetFullPath("LoadTestResult/Result.html"));
            }
        }

        protected virtual async Task<HttpClient> GetDefaultClient()
        {
            return _testOptions.DefaultHttpClient;
        }

        protected abstract string GetUrl(string path, string actionName);
        protected abstract void Initializer(IServiceProvider hostServices);
        protected abstract void ConfigureTestServices(IServiceCollection services);
    }

    public abstract class MarkopLoadTestFactory<TStartup, TFetchOption>
        : MarkopLoadTestFactory<TStartup, TFetchOption, MarkopLoadTestOptions>
        where TStartup : class
        where TFetchOption : class
    {
        protected MarkopLoadTestFactory(ITestOutputHelper outputHelper, MarkopLoadTestOptions loadTestOptions)
            : base(outputHelper, loadTestOptions)
        {
        }
    }

    public abstract class MarkopLoadTestFactory<TStartup>
        : MarkopLoadTestFactory<TStartup, dynamic, MarkopLoadTestOptions>
        where TStartup : class
    {
        protected MarkopLoadTestFactory(ITestOutputHelper outputHelper, MarkopLoadTestOptions loadTestOptions)
            : base(outputHelper, loadTestOptions)
        {
        }
    }
}