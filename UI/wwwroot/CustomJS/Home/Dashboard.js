
$(document).ready(function () {
    GetLogsStatistics();
});

var logsTrendChart = null;

function GetLogsStatistics() {
    var successCallbackFunction = function (result) {
        if (result.Header.Status != 1) {
            ShowAlert("error", "Error", "Failed to load logs statistics, please try again later!");
            return;
        }

        var data = result.Data;

        $('#statTotal').text(data.TotalCount);
        $('#statError').text(data.ErrorCount);
        $('#statWarning').text(data.WarningCount);
        $('#statInformation').text(data.InformationCount);

        RenderLogsTrendChart(data.DailyCounts || []);
    };

    CallAjaxMethod("POST", "/Home/GetLogsStatistics", {}, successCallbackFunction, "Oops something went wrong!");
}

function RenderLogsTrendChart(dailyCounts) {
    var countsByDate = {};
    (dailyCounts || []).forEach(function (item) {
        countsByDate[moment(item.Date).format('YYYY-MM-DD')] = item.Count;
    });

    var categories = [];
    var series = [];

    for (var i = 6; i >= 0; i--) {
        var day = moment().subtract(i, 'days');
        categories.push(day.format('MMM DD'));
        series.push(countsByDate[day.format('YYYY-MM-DD')] || 0);
    }

    var options = {
        chart: {
            type: 'bar',
            height: 300,
            toolbar: { show: false }
        },
        series: [{ name: 'Logs', data: series }],
        xaxis: { categories: categories },
        colors: ['#7239EA'],
        plotOptions: {
            bar: { borderRadius: 4, columnWidth: '40%' }
        },
        dataLabels: { enabled: false }
    };

    var el = document.querySelector('#logsTrendChart');
    if (!el) return;

    if (logsTrendChart) {
        logsTrendChart.updateOptions(options);
        return;
    }

    logsTrendChart = new ApexCharts(el, options);
    logsTrendChart.render();
}
