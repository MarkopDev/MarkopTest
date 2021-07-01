using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

            var tasks = new List<Task>();
            var syncRequestInfos = new ConcurrentDictionary<int, RequestInfo>();
            var asyncRequestInfos = new ConcurrentDictionary<int, RequestInfo>();

            for (var i = 0; i < 50; i++)
            {
                var sw = Stopwatch.StartNew();


                HttpResponseMessage response = null;

                try
                {
                    response = await client.PostAsync(_uri, JsonContent.Create(data));
                }
                catch (Exception e)
                {
                    // ignored
                }

                sw.Stop();

                var requestInfo = new RequestInfo
                {
                    ResponseStatus = response?.StatusCode ?? HttpStatusCode.InternalServerError,
                    ResponseTime = sw.ElapsedMilliseconds
                };

                while (!syncRequestInfos.TryAdd(i, requestInfo))
                {
                    Thread.Sleep(100);
                }
            }

            for (var i = 0; i < 1000; i++)
            {
                var i1 = i;
                tasks.Add(Task.Run(async () =>
                {
                    var sw = Stopwatch.StartNew();

                    HttpResponseMessage response = null;

                    try
                    {
                        response = await client.PostAsync(_uri, JsonContent.Create(data));
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }

                    sw.Stop();

                    var requestInfo = new RequestInfo
                    {
                        ResponseStatus = response?.StatusCode ?? HttpStatusCode.InternalServerError,
                        ResponseTime = sw.ElapsedMilliseconds
                    };

                    while (!asyncRequestInfos.TryAdd(i1, requestInfo))
                    {
                        Thread.Sleep(100);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            var syncRequestResponseTimes = syncRequestInfos.Values.Select(v => v.ResponseTime).ToArray();
            var asyncRequestResponseTimes = asyncRequestInfos.Values.Select(v => v.ResponseTime).ToArray();

            var syncAverage = syncRequestResponseTimes.Sum() / syncRequestResponseTimes.Count();
            var asyncAverage = asyncRequestResponseTimes.Sum() / asyncRequestResponseTimes.Count();

            _outputHelper.WriteLine(new string('-', 20));
            _outputHelper.WriteLine($"Sync Min: {syncRequestResponseTimes.Min(t => t) / 1000.0}Sec");
            _outputHelper.WriteLine($"Sync Average: {syncAverage / 1000.0}Sec");
            _outputHelper.WriteLine($"Sync Max: {syncRequestResponseTimes.Max(t => t) / 1000.0}Sec");
            _outputHelper.WriteLine($"Async Min: {asyncRequestResponseTimes.Min(t => t) / 1000.0}Sec");
            _outputHelper.WriteLine($"Async Average: {asyncAverage / 1000.0}Sec");
            _outputHelper.WriteLine($"Async Max: {asyncRequestResponseTimes.Max(t => t) / 1000.0}Sec");

            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Environment.CurrentDirectory)
                .UseMemoryCachingProvider()
                .Build();


            var syncTimesIterationArray = syncRequestResponseTimes.Select((l, i) => new[] {i, l});
            var asyncTimesIterationArray = asyncRequestResponseTimes.Select((l, i) => new[] {i, l});

            var syncTimesDistributionArray = syncRequestResponseTimes.GroupBy(value => value)
                .Select(group => { return new[] {group.Key, group.Count()}; })
                .OrderBy(x => x[1]);
            var asyncTimesDistributionArray = asyncRequestResponseTimes.GroupBy(value => value)
                .Select(group => { return new[] {group.Key, group.Count()}; })
                .OrderBy(x => x[1]);

            var syncResponseStatus = syncRequestInfos.Values.GroupBy(value => value.ResponseStatus)
                .Select(group => { return new dynamic[] {group.Key.ToString(), group.Count()}; });
            var asyncResponseStatus = asyncRequestInfos.Values.GroupBy(value => value.ResponseStatus)
                .Select(group => { return new dynamic[] {group.Key.ToString(), group.Count()}; });


            dynamic[][] getSummaryRange(long[] summaryData, int rangeCount)
            {
                var min = summaryData.Min();
                var max = summaryData.Max();
                var step = (max - min) / rangeCount;
                var summaryRanges = new long[rangeCount];
                var summaryRangeTitles = new string[rangeCount];

                for (var i = min; i < max; i += step)
                {
                    var index = (i - min) / step;
                    index = index == rangeCount ? rangeCount - 1 : index;
                    summaryRangeTitles[index] = $"t < {i + step}ms";
                    if (i != min)
                        summaryRangeTitles[index] = $"{i}ms < " + summaryRangeTitles[index];
                }

                foreach (var d in summaryData)
                    summaryRanges[(d - min) / step == rangeCount ? rangeCount - 1 : (d - min) / step] += 1;

                return summaryRangeTitles.Select((v, i) => new dynamic[] {v, summaryRanges[i]}).ToArray();
            }

            var syncSummaryRanges = getSummaryRange(syncRequestResponseTimes, 5);
            var asyncSummaryRanges = getSummaryRange(asyncRequestResponseTimes, 5);

            var model = new ExportResultModel
            {
                SyncSummaryRange = JsonSerializer.Serialize(syncSummaryRanges),
                AsyncSummaryRange = JsonSerializer.Serialize(asyncSummaryRanges),
                SyncResponseStatus = JsonSerializer.Serialize(syncResponseStatus),
                AsyncResponseStatus = JsonSerializer.Serialize(asyncResponseStatus),
                SyncTimesIterationsJsArray = JsonSerializer.Serialize(syncTimesIterationArray),
                AsyncTimesIterationsJsArray = JsonSerializer.Serialize(asyncTimesIterationArray),
                SyncTimesDistributionJsArray = JsonSerializer.Serialize(syncTimesDistributionArray),
                AsyncTimesDistributionJsArray = JsonSerializer.Serialize(asyncTimesDistributionArray),
            };

            var result = await engine.CompileRenderAsync("Template/Result.cshtml", model);

            if (Directory.Exists("LoadTestResult"))
                Directory.Delete("LoadTestResult", true);

            Directory.CreateDirectory("LoadTestResult");

            void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
            {
                foreach (DirectoryInfo dir in source.GetDirectories())
                    CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
                foreach (FileInfo file in source.GetFiles())
                    file.CopyTo(Path.Combine(target.FullName, file.Name));
            }

            var source = new DirectoryInfo("Template");
            var target = new DirectoryInfo("LoadTestResult");
            CopyFilesRecursively(source, target);

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