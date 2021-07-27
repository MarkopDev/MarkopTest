function setup() {
    styling()
    
    setInfo('os', data.OS)
    setInfo('api', data.ApiUrl)
    setInfo('cpu', data.CpuName)
    setInfo('sync-request-count', data.SyncRequestCount)
    setInfo('async-request-count', data.AsyncRequestCount)
    setInfo('ram', `${data.RamSize / 1024 / 1024 / 1024} GB`)
    setInfo('sync-max-response-time', formatTime(data.SyncMaxResponseTime))
    setInfo('sync-avg-response-time', formatTime(data.SyncAvgResponseTime))
    setInfo('sync-min-response-time', formatTime(data.SyncMinResponseTime))
    setInfo('async-max-response-time', formatTime(data.AsyncMaxResponseTime))
    setInfo('async-avg-response-time', formatTime(data.AsyncAvgResponseTime))
    setInfo('async-min-response-time', formatTime(data.AsyncMinResponseTime))

    setupResponseTime()
    setupSummaryRange()
    setupMemorySamples()
    setupResponseStatus()
    setupDistributionTime()
}

function styling() {
    [...document.getElementsByClassName('mk-info-value')].forEach(el => {
        el.style = `background: ${data.BaseColor};`
    });
    [...document.getElementsByClassName('mk-info-box-value')].forEach(el => {
        el.style = `background: ${data.BaseColor};`
    });
}

function setInfo(key, value) {
    document.getElementById(`info-${key}`).innerHTML = value
}

function formatTime(value) {
    if (value < 1000)
        return `${value} ms`
    return `${value / 1000} sec`
}