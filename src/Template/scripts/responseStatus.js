var options = {
    series: asyncResponseStatus.map(d => d[1]),
    chart: {
        height: 350,
        type: 'polarArea'
    },
    labels: asyncResponseStatus.map(d => d[0]),
    fill: {
        opacity: 1
    },
    title: {
        text: 'Async Request, Response Status',
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
    },
    theme: {
        monochrome: {
            enabled: true,
            shadeTo: 'light',
            shadeIntensity: 0.6
        }
    }
};

new ApexCharts(document.querySelector("#response_status_async"), options).render();
new ApexCharts(document.querySelector("#response_status_sync"), {
    ...options,
    series: syncResponseStatus.map(d => d[1]),
    labels: syncResponseStatus.map(d => d[0]),
    title: {
        text: 'Sync Request, Response Status',
    }}).render();