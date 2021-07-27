function setupResponseStatus() {
    let options = {
        series: data.AsyncResponseStatus.map(d => d.Count),
        labels: data.AsyncResponseStatus.map(d => d.Status),
        chart: {
            height: 350,
            type: 'donut'
        },
        colors: [data.BaseColor],
        fill: {
            opacity: 1
        },
        title: {
            text: 'Response status (async)',
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
        series: data.SyncResponseStatus.map(d => d.Count),
        labels: data.SyncResponseStatus.map(d => d.Status),
        title: {
            text: 'Response status (sync)',
        }
    }).render();
}