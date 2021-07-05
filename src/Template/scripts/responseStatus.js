var options = {
    series: asyncResponseStatus.map(d => d[1]),
    labels: asyncResponseStatus.map(d => d[0]),
    chart: {
        height: 350,
        type: 'polarArea'
    },
    colors: [chartColor],
    fill: {
        opacity: 1
    },
    title: {
        text: 'Response Status of Async Request',
    },
    stroke: {
        width: 1,
        colors: undefined
    },
    yaxis: {
        show: false
    },
    legend: {
        position: 'bottom'
    },
    plotOptions: {
        polarArea: {
            rings: {
                strokeWidth: 0
            },
            spokes: {
                strokeWidth: 0
            },
        }
    }
};

new ApexCharts(document.querySelector("#response_status_async"), options).render();
new ApexCharts(document.querySelector("#response_status_sync"), {
    ...options,
    series: syncResponseStatus.map(d => d[1]),
    labels: syncResponseStatus.map(d => d[0]),
    title: {
        text: 'Response Status of Sync Request',
    }}).render();