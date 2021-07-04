var options = {
    series: [{
        name: 'Async Request',
        data: asyncTimesDistributionJsArray
    }],
    chart: {
        type: 'bar',
        height: 350,
    },
    dataLabels: {
        enabled: false,
    },
    title: {
        text: 'Async Request Response Time Distribution',
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
        name: 'Sync Request',
        data: syncTimesDistributionJsArray
    }], title: {
        text: 'Sync Request Response Time Distribution',
    }}).render();