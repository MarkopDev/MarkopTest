var options = {
    series: [{
        name: 'Sync Memory Samples',
        data: syncMemorySamples
    }],
    chart: {
        type: 'area',
        height: 350,
    },
    colors: [chartColor],
    dataLabels: {
        enabled: false,
    },
    title: {
        text: 'Sync Request Memory Per Iteration',
    },
    yaxis: {
        type: 'numeric',
        labels: {
            formatter: function (val) {
                return (val / 1024 / 1024).toFixed(2) + ' MB'
            }
        },
        title: {
            text: 'Memory'
        },
    },
    xaxis: {
        type: 'numeric',
        title: {
            text: 'Iteration'
        },
        labels: {
            formatter: function (val) {
                return val
            }
        },
    },
    tooltip: {
        shared: false,
        y: {
            formatter: function (val) {
                return (val / 1024 / 1024).toFixed(3) + ' MB'
            }
        }
    }
};

new ApexCharts(document.querySelector("#memory_sync"), options).render();
new ApexCharts(document.querySelector("#memory_async"), {
    ...options, series: [{
        name: 'Async Memory Samples',
        data: asyncMemorySamples
    }],
    title: {
        text: 'Async Request Memory Per Iteration',
    }
}).render();
