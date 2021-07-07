﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hardware.Info;
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

        protected async Task Post(dynamic data, HttpClient client = null,
            TFetchOptions fetchOptions = null)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != Environments.Development)
                return;

            client ??= await GetDefaultClient() ?? Client;

            var tasks = new List<Task>();
            var syncMemorySamples = new ConcurrentDictionary<int, long>();
            var asyncMemorySamples = new ConcurrentDictionary<int, long>();
            var syncRequestInfos = new ConcurrentDictionary<int, RequestInfo>();
            var asyncRequestInfos = new ConcurrentDictionary<int, RequestInfo>();

            for (var i = 0; i < _testOptions.SyncRequestCount; i++)
            {
                var content = new StringContent(JsonSerializer.Serialize(data), Encoding.Default, "application/json");

                var beforeRequestMemory = Process.GetCurrentProcess().PrivateMemorySize64;

                var sw = Stopwatch.StartNew();

                HttpResponseMessage response = null;

                try
                {
                    response = await client.PostAsync(_uri, content);
                }
                catch (Exception)
                {
                    // ignored
                }

                sw.Stop();

                var memorySize = Process.GetCurrentProcess().PrivateMemorySize64 - beforeRequestMemory;

                if (memorySize < 0)
                    memorySize = 0;

                while (!syncMemorySamples.TryAdd(i, memorySize))
                {
                    Thread.Sleep(100);
                }

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

            for (var i = 0; i < _testOptions.AsyncRequestCount; i++)
            {
                var i1 = i;
                tasks.Add(await Task.Run<Task>(async () =>
                {
                    var content = new StringContent(JsonSerializer.Serialize(data),
                        Encoding.Default, "application/json");

                    var sw = Stopwatch.StartNew();

                    HttpResponseMessage response = null;

                    try
                    {
                        response = await client.PostAsync(_uri, content);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    sw.Stop();

                    while (!asyncMemorySamples.TryAdd(i1, Process.GetCurrentProcess().PrivateMemorySize64))
                    {
                        Thread.Sleep(100);
                    }

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

            var minAsyncMemoryTrend = asyncMemorySamples.Values.Min();
            var asyncMemoryTrend = asyncMemorySamples.Values.Select(v => v - minAsyncMemoryTrend).ToArray();

            var syncMemoryTrend = syncMemorySamples.Values.ToArray();

            var syncRequestResponseTimes = syncRequestInfos.Values.Select(v => v.ResponseTime).ToArray();
            var asyncRequestResponseTimes = asyncRequestInfos.Values.Select(v => v.ResponseTime).ToArray();

            var syncAverage = syncRequestResponseTimes.Sum() / syncRequestResponseTimes.Count();
            var asyncAverage = asyncRequestResponseTimes.Sum() / asyncRequestResponseTimes.Count();

            _outputHelper.WriteLine(new string('-', 20));
            _outputHelper.WriteLine($"Sync Min: {syncRequestResponseTimes.Min(t => t) / 1000.0} sec");
            _outputHelper.WriteLine($"Sync Average: {syncAverage / 1000.0} sec");
            _outputHelper.WriteLine($"Sync Max: {syncRequestResponseTimes.Max(t => t) / 1000.0} sec");
            _outputHelper.WriteLine($"Async Min: {asyncRequestResponseTimes.Min(t => t) / 1000.0} sec");
            _outputHelper.WriteLine($"Async Average: {asyncAverage / 1000.0} sec");
            _outputHelper.WriteLine($"Async Max: {asyncRequestResponseTimes.Max(t => t) / 1000.0} sec");

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


            dynamic[][] GetSummaryRange(long[] summaryData, int rangeCount)
            {
                var min = summaryData.Min();
                var max = summaryData.Max();
                var step = (max - min) / rangeCount;

                if (step == 0)
                    return new[]
                    {
                        new dynamic[] {$"{min} < t < {max}", summaryData.Length}
                    };

                var summaryRanges = new long[rangeCount];
                var summaryRangeTitles = new string[rangeCount];

                for (var i = 0; i < rangeCount; i++)
                    summaryRangeTitles[i] = (i == 0 ? "" : $"{i * step}ms < ") + "t" +
                                            (i == rangeCount - 1 ? "" : $" < {(i + 1) * step}ms");

                foreach (var d in summaryData)
                    summaryRanges[(d - min) / step >= rangeCount ? rangeCount - 1 : (d - min) / step]++;

                return summaryRangeTitles.Select((v, i) => new dynamic[] {v, summaryRanges[i]}).ToArray();
            }


            var syncSummaryRanges = GetSummaryRange(syncRequestResponseTimes, 5);
            var asyncSummaryRanges = GetSummaryRange(asyncRequestResponseTimes, 5);

            var hardwareInfo = new HardwareInfo();
            
            hardwareInfo.RefreshCPUList();
            hardwareInfo.RefreshMemoryList();

            var model = new ExportResultModel
            {
                ApiUrl = _uri,
                ChartColor = _testOptions.ChartColor,
                SyncRequestCount = _testOptions.SyncRequestCount,
                AsyncRequestCount = _testOptions.AsyncRequestCount,
                SyncAvgResponseTime = $"{syncAverage / 1000.0:0.00}",
                AsyncAvgResponseTime = $"{asyncAverage / 1000.0:0.00}",
                SyncMemorySamples = JsonSerializer.Serialize(syncMemoryTrend),
                SyncSummaryRange = JsonSerializer.Serialize(syncSummaryRanges),
                AsyncMemorySamples = JsonSerializer.Serialize(asyncMemoryTrend),
                AsyncSummaryRange = JsonSerializer.Serialize(asyncSummaryRanges),
                SyncResponseStatus = JsonSerializer.Serialize(syncResponseStatus),
                AsyncResponseStatus = JsonSerializer.Serialize(asyncResponseStatus),
                OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                RamSize = $"{hardwareInfo.MemoryList.Sum(m => (long)m.Capacity) / 1024.0 / 1024.0 / 1024.0:0}",
                SyncTimesIterationsJsArray = JsonSerializer.Serialize(syncTimesIterationArray),
                AsyncTimesIterationsJsArray = JsonSerializer.Serialize(asyncTimesIterationArray),
                SyncTimesDistributionJsArray = JsonSerializer.Serialize(syncTimesDistributionArray),
                AsyncTimesDistributionJsArray = JsonSerializer.Serialize(asyncTimesDistributionArray),
                SyncMinResponseTime = $"{syncRequestResponseTimes.Min(t => t) / 1000.0:0.00}",
                SyncMaxResponseTime = $"{syncRequestResponseTimes.Max(t => t) / 1000.0:0.00}",
                AsyncMinResponseTime = $"{asyncRequestResponseTimes.Min(t => t) / 1000.0:0.00}",
                AsyncMaxResponseTime = $"{asyncRequestResponseTimes.Max(t => t) / 1000.0:0.00}",
                CpuName = string.Join(", ", hardwareInfo.CpuList.Select(c => c.Name).ToArray()),
            };

            var result = await engine.CompileRenderAsync("Template/Result.cshtml", model);

            if (Directory.Exists("LoadTestResult"))
                Directory.Delete("LoadTestResult", true);

            Directory.CreateDirectory("LoadTestResult");

            static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
            {
                foreach (var dir in source.GetDirectories())
                    CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
                foreach (var fileInfo in source.GetFiles())
                    fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name));
            }

            CopyFilesRecursively(new DirectoryInfo("Template"), new DirectoryInfo("LoadTestResult"));

            await using var file = File.Create("LoadTestResult/Result.html");
            file.Write(Encoding.UTF8.GetBytes(result));

            await using var jsonFile = File.Create("LoadTestResult/data.json");
            jsonFile.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(model)));


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