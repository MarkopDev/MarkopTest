function setupResponseTime() {
    let options = {
        series: [{
            name: 'Response time',
            data: data.SyncTimesIterations
        }],
        chart: {
            type: 'line',
            height: 350,
        },
        colors: [data.ChartColor],
        dataLabels: {
            enabled: false,
        },
        grid: {
            xaxis: {
                lines: {
                    show: true,
                }
            },
        },
        title: {
            text: 'Response time per iteration (sync)',
        },
        yaxis: {
            type: 'numeric',
            labels: {
                formatter: function (val) {
                    return val;
                },
            },
            title: {
                text: 'Time(ms)'
            },
        },
        xaxis: {
            type: 'numeric',
            title: {
                text: 'Iteration'
            },
        },
        tooltip: {
            shared: false,
            y: {
                formatter: function (val) {
                    return (val / 1000) + ' second'
                }
            }
        }
    };

    new ApexCharts(document.querySelector("#response_time_sync"), options).render();
    new ApexCharts(document.querySelector("#response_time_async"), {
        ...options, series: [{
            name: 'Response time',
            data: data.AsyncTimesIterations
        }],
        title: {
            text: 'Response time per iteration (async)',
        }
    }).render();
}