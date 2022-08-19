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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Hardware.Info;
using MarkopTest.Attributes;
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

        private IServiceProvider _serviceProvider;
        protected readonly TTestOptions TestOptions;
        private readonly ITestOutputHelper _outputHelper;
        protected IServiceProvider Services => _serviceProvider.CreateScope().ServiceProvider;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly ManualResetEventSlim _initializationTask = new ManualResetEventSlim(false);

        protected LoadTestFactory(ITestOutputHelper outputHelper, TTestOptions testOptions = null)
        {
            _outputHelper = outputHelper;
            TestOptions = testOptions ?? new TTestOptions();
        }

        private IHost Host => TestOptions.HostSeparation ? _seperatedHost : _host;

        private async void InitializeHost()
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

            await Host.WaitForShutdownAsync();
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

        protected HttpClient GetClient()
        {
            new Thread(InitializeHost).Start();
            _initializationTask.Wait(-1);

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

        // TODO add put patch delete and other type of the http method
        protected void PostJson(dynamic data, HttpClient client = null,
            TFetchOptions fetchOptions = null)
        {
            var url = GetUrl();
            
            var content = new StringContent(JsonSerializer.Serialize(data),
                Encoding.Default, "application/json");

            PostAsync(url, content, client, fetchOptions).GetAwaiter().GetResult();
        }

        protected async Task PostAsync(string url, HttpContent content, HttpClient client = null,
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
                    response = await client.PostAsync(url, content);
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
                        response = await client.PostAsync(url, content);
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

            var syncTimesIterationArray = syncRequestResponseTimes.Select((l, i) => new[] {i, l}).ToArray();
            var asyncTimesIterationArray = asyncRequestResponseTimes.Select((l, i) => new[] {i, l}).ToArray();

            var syncTimesDistributionArray = syncRequestResponseTimes.GroupBy(value => value)
                .Select(group => { return new[] {group.Key, group.Count()}; })
                .OrderBy(x => x[1]).ToArray();
            var asyncTimesDistributionArray = asyncRequestResponseTimes.GroupBy(value => value)
                .Select(group => { return new[] {group.Key, group.Count()}; })
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
                ApiUrl = url,
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
                RamSize = hardwareInfo.MemoryList.Sum(m => (long) m.Capacity),
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
        
        protected abstract Task Initializer(IServiceProvider hostServices);
        protected abstract void ConfigureTestServices(IServiceCollection services);
        protected abstract string GetUrl(string url, string controllerName, string testMethodName);
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