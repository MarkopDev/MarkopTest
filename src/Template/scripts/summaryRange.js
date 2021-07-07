var options = {
    series: [{
        name: 'Request count',
        data: asyncSummaryRange.map(d => d[1])
    }],
    labels: asyncSummaryRange.map(d => d[0]),
    chart: {
        type: 'bar',
        height: 350,
    },
    colors: [chartColor],
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
        data: syncSummaryRange.map(d => d[1])
    }],
    labels: syncSummaryRange.map(d => d[0]),
    title: {
        text: 'Response time (sync)',
    }}).render();