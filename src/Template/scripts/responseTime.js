var options = {
    series: [{
        name: 'Sync Request',
        data: syncTimesIterationsJsArray
    }],
    chart: {
        type: 'line',
        height: 350,
    },
    colors: [chartColor],
    dataLabels: {
        enabled: false,
    },
    title: {
        text: 'Sync Request Response Time Per Iteration',
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
        name: 'Async Request',
        data: asyncTimesIterationsJsArray
    }],
    title: {
        text: 'Async Request Response Time Per Iteration',
    }
}).render();
