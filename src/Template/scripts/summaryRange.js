function setupSummaryRange() {
    let options = {
        series: [{
            name: 'Request count',
            data: data.AsyncTimeSummaryRange.map(d => d.Count)
        }],
        labels: data.AsyncTimeSummaryRange.map(d => d.Range),
        chart: {
            type: 'bar',
            height: 350,
        },
        colors: [data.BaseColor],
        dataLabels: {
            enabled: false,
        },
        title: {
            text: 'Response time (async)',
        },
        yaxis: {
            type: 'numeric',
            labels: {
                formatter: function (val) {
                    return val;
                },
            },
            title: {
                text: 'Request Count'
            },
        },
        xaxis: {
            title: {
                text: 'Time Range(ms)'
            },
        },
        tooltip: {
            shared: false,
            y: {
                formatter: function (val) {
                    return val
                }
            }
        }
    };

    new ApexCharts(document.querySelector("#summary_rang_async"), options).render();
    new ApexCharts(document.querySelector("#summary_rang_sync"), {...options,
        series: [{
            name: 'Request count',
            data: data.SyncTimeSummaryRange.map(d => d.Count)
        }],
        labels: data.SyncTimeSummaryRange.map(d => d.Range),
        title: {
            text: 'Response time (sync)',
        }}).render();
}