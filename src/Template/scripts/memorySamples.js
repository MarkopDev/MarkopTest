﻿function setupMemorySamples() {
    let options = {
        series: [{
            name: 'Memory Size',
            data: data.SyncMemorySamples
        }],
        chart: {
            type: 'area',
            height: 350,
        },
        colors: [data.BaseColor],
        dataLabels: {
            enabled: false,
        },
        title: {
            text: 'Memory per sample (sync)',
        },
        grid: {
            xaxis: {
                lines: {
                    show: true,
                }
            },
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
                text: 'Sample'
            },
            labels: {
                formatter: function (val) {
                    return Math.round(val)
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
            name: 'Memory Size',
            data: data.AsyncMemorySamples
        }],
        title: {
            text: 'Memory per sample (async)',
        }
    }).render();
}