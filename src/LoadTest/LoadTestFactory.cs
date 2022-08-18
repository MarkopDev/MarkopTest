using System;
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
using Xunit.Abstractions;

namespace MarkopTest.LoadTest
{
    public abstract class LoadTestFactory<TStartup, TFetchOptions, TTestOptions>
        where TStartup : class
        where TFetchOptions : class
        where TTestOptions : LoadTestOptions, new()
    {
        private static IHost _host;

        private IHost _seperatedHost;

        // for passing the parameters in tests
        private IServiceProvider _serviceProvider;

        public readonly string Uri;
        protected readonly TTestOptions TestOptions;
        private readonly ITestOutputHelper _outputHelper;
        protected IServiceProvider Services => _serviceProvider.CreateScope().ServiceProvider;

        protected LoadTestFactory(ITestOutputHelper outputHelper, TTestOptions testOptions = null)
        {
            _outputHelper = outputHelper;
            TestOptions = testOptions ?? new TTestOptions();

            var initial = new StackTrace().GetFrame(4)?.GetMethod()?.Name == "InvokeMethod" ||
                          new StackTrace().GetFrame(3)?.GetMethod()?.Name == "InvokeMethod";

            // TODO update initializer like Integration Test
            if (initial)
                ConfigureWebHost();

            if (initial && Host != null)
            {
                Initializer(Host.Services);
                _serviceProvider = Host.Services;
            }

            // TODO calculate URL using endpoint attribute

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

        // TODO add put patch delete and other type of the http method
        protected async Task PostJsonAsync(dynamic data, HttpClient client = null,
            TFetchOptions fetchOptions = null)
        {
            var content = new StringContent(JsonSerializer.Serialize(data),
                Encoding.Default, "application/json");

            await PostAsync(content, client, fetchOptions);
        }

        protected async Task PostAsync(HttpContent content, HttpClient client = null,
            TFetchOptions fetchOptions = null)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != Environments.Development)
                return;

            client ??= GetClient();

            // TODO handle handler like Integration Test
            // await TestHandlerHelper.BeforeRequest(client, typeof(LoadTestFactory<>));

            var tasks = new List<Task>();
            var syncMemorySamples = new ConcurrentDictionary<int, long>();
            var asyncMemorySamples = new ConcurrentDictionary<int, long>();
            var syncRequestInfos = new ConcurrentDictionary<int, RequestInfo>();
            var asyncRequestInfos = new ConcurrentDictionary<int, RequestInfo>();

            for (var i = 0; i < TestOptions.SyncRequestCount; i++)
            {
                var beforeRequestMemory = Process.GetCurrentProcess().PrivateMemorySize64;

                var sw = Stopwatch.StartNew();

                HttpResponseMessage response = null;

                try
                {
                    response = await client.PostAsync(Uri, content);
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

            for (var i = 0; i < TestOptions.AsyncRequestCount; i++)
            {
                var i1 = i;
                tasks.Add(await Task.Run<Task>(async () =>
                {
                    var sw = Stopwatch.StartNew();

                    HttpResponseMessage response = null;

                    try
                    {
                        response = await client.PostAsync(Uri, content);
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

            // TODO handle handler like Integration Test
            // await TestHandlerHelper.AfterRequest(client, typeof(LoadTestFactory<>));

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

            var syncTimesIterationArray = syncRequestResponseTimes.Select((l, i) => new[] { i, l }).ToArray();
            var asyncTimesIterationArray = asyncRequestResponseTimes.Select((l, i) => new[] { i, l }).ToArray();

            var syncTimesDistributionArray = syncRequestResponseTimes.GroupBy(value => value)
                .Select(group => { return new[] { group.Key, group.Count() }; })
                .OrderBy(x => x[1]).ToArray();
            var asyncTimesDistributionArray = asyncRequestResponseTimes.GroupBy(value => value)
                .Select(group => { return new[] { group.Key, group.Count() }; })
                .OrderBy(x => x[1]).ToArray();

            var syncResponseStatus = syncRequestInfos.Values.GroupBy(value => value.ResponseStatus)
                .Select(group => new ResponseStatusResult
                {
                    Status = group.Key.ToString(),
                    Count = group.Count()
                }).ToArray();
            var asyncResponseStatus = asyncRequestInfos.Values.GroupBy(value => value.ResponseStatus)
                .Select(group => new ResponseStatusResult
                {
                    Status = group.Key.ToString(),
                    Count = group.Count()
                }).ToArray();


            ResponseTimeSummaryResult[] GetTimeSummaryRange(IReadOnlyCollection<long> summaryData, int rangeCount)
            {
                var min = summaryData.Min();
                var max = summaryData.Max();
                var step = (max - min) / rangeCount;

                if (step == 0)
                    return new[]
                    {
                        new ResponseTimeSummaryResult
                        {
                            Range = $"{min} < t < {max}",
                            Count = summaryData.Count
                        }
                    };

                var summaryRanges = new int[rangeCount];
                var summaryRangeTitles = new string[rangeCount];

                for (var i = 0; i < rangeCount; i++)
                    summaryRangeTitles[i] = (i == 0 ? "" : $"{i * step}ms < ") + "t" +
                                            (i == rangeCount - 1 ? "" : $" < {(i + 1) * step}ms");

                foreach (var d in summaryData)
                    summaryRanges[(d - min) / step >= rangeCount ? rangeCount - 1 : (d - min) / step]++;

                return summaryRangeTitles.Select((v, i) =>
                    new ResponseTimeSummaryResult
                    {
                        Range = v,
                        Count = summaryRanges[i]
                    }).ToArray();
            }


            var syncTimeSummaryRanges = GetTimeSummaryRange(syncRequestResponseTimes, 5);
            var asyncTimeSummaryRanges = GetTimeSummaryRange(asyncRequestResponseTimes, 5);

            var hardwareInfo = new HardwareInfo();

            hardwareInfo.RefreshCPUList();
            hardwareInfo.RefreshMemoryList();

            var model = new ExportResultModel
            {
                ApiUrl = Uri,
                SyncAvgResponseTime = syncAverage,
                BaseColor = TestOptions.BaseColor,
                AsyncAvgResponseTime = asyncAverage,
                SyncMemorySamples = syncMemoryTrend,
                AsyncMemorySamples = asyncMemoryTrend,
                SyncResponseStatus = syncResponseStatus,
                AsyncResponseStatus = asyncResponseStatus,
                SyncTimeSummaryRange = syncTimeSummaryRanges,
                SyncTimesIterations = syncTimesIterationArray,
                AsyncTimeSummaryRange = asyncTimeSummaryRanges,
                AsyncTimesIterations = asyncTimesIterationArray,
                SyncRequestCount = TestOptions.SyncRequestCount,
                AsyncRequestCount = TestOptions.AsyncRequestCount,
                SyncTimesDistribution = syncTimesDistributionArray,
                AsyncTimesDistribution = asyncTimesDistributionArray,
                SyncMinResponseTime = syncRequestResponseTimes.Min(),
                SyncMaxResponseTime = syncRequestResponseTimes.Max(),
                AsyncMinResponseTime = asyncRequestResponseTimes.Min(),
                AsyncMaxResponseTime = asyncRequestResponseTimes.Max(),
                OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                RamSize = hardwareInfo.MemoryList.Sum(m => (long)m.Capacity),
                CpuName = string.Join(", ", hardwareInfo.CpuList.Select(c => c.Name).ToArray()),
            };

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

            await using var jsFile = File.Create("LoadTestResult/data.js");
            jsFile.Write(Encoding.UTF8.GetBytes($"var data = JSON.parse('{JsonSerializer.Serialize(model)}');"));

            if (TestOptions.OpenResultAfterFinished)
            {
                Process.Start(@"cmd.exe", @"/c " + Path.GetFullPath("LoadTestResult/Result.html"));
            }
        }

        protected HttpClient GetClient()
        {
            if (TestOptions.BaseAddress == null)
                return Host.GetTestClient();

            var httpClientHandler = new HttpClientHandler
            {
                Proxy = TestOptions.Proxy
            };
            return new HttpClient(httpClientHandler)
            {
                BaseAddress = TestOptions.BaseAddress
            };
        }

        protected abstract string GetUrl(string path, string actionName);
        protected abstract void Initializer(IServiceProvider hostServices);
        protected abstract void ConfigureTestServices(IServiceCollection services);
    }

    public abstract class LoadTestFactory<TStartup, TFetchOption>
        : LoadTestFactory<TStartup, TFetchOption, LoadTestOptions>
        where TStartup : class
        where TFetchOption : class
    {
        protected LoadTestFactory(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected LoadTestFactory(ITestOutputHelper outputHelper, LoadTestOptions loadTestOptions)
            : base(outputHelper, loadTestOptions)
        {
        }
    }

    public abstract class LoadTestFactory<TStartup>
        : LoadTestFactory<TStartup, dynamic, LoadTestOptions>
        where TStartup : class
    {
        protected LoadTestFactory(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected LoadTestFactory(ITestOutputHelper outputHelper, LoadTestOptions loadTestOptions)
            : base(outputHelper, loadTestOptions)
        {
        }
    }
}