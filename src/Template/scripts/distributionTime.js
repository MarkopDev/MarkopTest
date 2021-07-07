var options = {
    series: [{
        name: 'Request count',
        data: asyncTimesDistributionJsArray
    }],
    chart: {
        type: 'bar',
        height: 350,
    },
    colors: [chartColor],
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
        text: 'Response time distribution (async)',
    },
    yaxis: {
        type: 'numeric',
        title: {
            text: 'Request Count'
        },
    },
    xaxis: {
        type: 'numeric',
        title: {
            text: 'Time(ms)'
        },
    },
    tooltip: {
        shared: false,
        x: {
            formatter: function (val) {
                return (val / 1000) + ' second'
            }
        },
        y: {
            formatter: function (val) {
                return val + ' count'
            }
        }
    }
};

new ApexCharts(document.querySelector("#distribution_time_async"), options).render();
new ApexCharts(document.querySelector("#distribution_time_sync"), {...options, series:[{
        name: 'Request Count',
        data: syncTimesDistributionJsArray
    }], title: {
        text: 'Response time distribution (sync)',
    }}).render();