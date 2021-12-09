using System;
using System.Net;

namespace MarkopTest.LoadTest;

public class LoadTestOptions
{
    public WebProxy Proxy { get; set; }
    public Uri BaseAddress { get; set; }
    public int SyncRequestCount { get; set; } = 50;
    public bool HostSeparation { get; set; } = false;
    public int AsyncRequestCount { get; set; } = 1000;
    public string BaseColor { get; set; } = "#427bcb";
    public bool OpenResultAfterFinished { get; set; } = true;
}