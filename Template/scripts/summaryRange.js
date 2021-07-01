var options = {
    series: [{
        name: 'Async Request',
        data: asyncSummaryRange.map(d => d[1])
    }],
    chart: {
        type: 'bar',
        height: 350,
    },
    labels: asyncSummaryRange.map(d => d[0]),
    dataLabels: {
        enabled: false,
    },
    title: {
        text: 'Async Request Summary Response Time',
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
        name: 'Sync Request',
        data: syncSummaryRange.map(d => d[1])
    }],
    labels: syncSummaryRange.map(d => d[0]),
    title: {
        text: 'Sync Request Summary Response Time',
    }}).render();