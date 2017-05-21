var labelsArray = "@ViewBag.ColumnName".split('|');
var dataTempHum =
    {
        labels: labelsArray,
        datasets:
            [{
                label: "Temperatura (ºC)",
                fillColor: "rgba(255,200,200,0.25)",
                strokeColor: "rgba(255,150,150,1)",
                pointColor: "rgba(255,50,50,1)",
                pointStrokeColor: "#fff",
                pointHighlightFill: "#fff",
                pointHighlightStroke: "rgba(255,0,0,1)",
                data: "@ViewBag.ValuesTemperatura".split('|')
            }
                ,
            {
                label: "Humedad (%)",
                fillColor: "rgba(200,200,255,0.25)",
                strokeColor: "rgba(150,150,255,1)",
                pointColor: "rgba(50,50,255,1)",
                pointStrokeColor: "#fff",
                pointHighlightFill: "#fff",
                pointHighlightStroke: "rgba(0,0,255,1)",
                data: "@ViewBag.ValuesHumedad".split('|')
            }]
    }

var dataCO =
    {
        labels: labelsArray,
        datasets:
            [{
                label: "Óxido de Carbono (CO)",
                fillColor: "rgba(200,200,200,0.5)",
                strokeColor: "rgba(180,180,180,1)",
                pointColor: "rgba(150,150,150,1)",
                pointStrokeColor: "#fff",
                pointHighlightFill: "#fff",
                pointHighlightStroke: "rgba(0,0,0,1)",
                data: "@ViewBag.ValuesCO".split('|')
            }]
    }

var dataConsumo =
    {
        labels: labelsArray,
        datasets:
            [{
                label: "Consumo (w)",
                fillColor: "rgba(255,127,0,0.5)",
                strokeColor: "rgba(255,150,40,1)",
                pointColor: "rgba(255,127,0,1)",
                pointStrokeColor: "#fff",
                pointHighlightFill: "#fff",
                pointHighlightStroke: "rgba(255,127,0,1)",
                data: "@ViewBag.ValuesConsumo".split('|')
            }]
    }
var options = {
    ///Boolean - Whether grid lines are shown across the chart
    scaleShowGridLines: true,
    //String - Colour of the grid lines
    scaleGridLineColor: "rgba(0,0,0,.05)",
    //Number - Width of the grid lines
    scaleGridLineWidth: 1,
    //Boolean - Whether the line is curved between points
    bezierCurve: true,
    //Number - Tension of the bezier curve between points
    bezierCurveTension: 0.4,
    //Boolean - Whether to show a dot for each point
    pointDot: true,
    //Number - Radius of each point dot in pixels
    pointDotRadius: 4,
    //Number - Pixel width of point dot stroke
    pointDotStrokeWidth: 1,
    //Number - amount extra to add to the radius to cater for hit detection outside the drawn point
    pointHitDetectionRadius: 6,
    //Boolean - Whether to show a stroke for datasets
    datasetStroke: true,
    //Number - Pixel width of dataset stroke
    datasetStrokeWidth: 2,
    //Boolean - Whether to fill the dataset with a colour
    datasetFill: true,
    zoomEnabled: false,
}
//console.log(document.getElementById("myChart"));

//Temperatura y Humedad
//function drawTempHum() {
var ctx = document.getElementById("myChartTempHum").getContext("2d");
/*var chartTempHum = */new Chart(ctx).Line(dataTempHum, options);
//chartTempHum.render();
//}
//CO
function drawCO() {
    var ctx = document.getElementById("myChartCO").getContext("2d");
    var charCO = new Chart(ctx).Line(dataCO, options);
    charCO.render();
}
//Consumo
function drawConsumo() {
    var ctx = document.getElementById("myChartConsumo").getContext("2d");
    var chartConsumo = new Chart(ctx).Line(dataConsumo, options);
    chartConsumo.render();
}

/*$('#bs-grafTempHum').on("shown.bs.tab", function () {
    drawTempHum();
    $('#bs-grafTempHum').off(); //to remove the binded event after initial rendering
});*/
$('#bs-grafCO').on("shown.bs.tab", function () {
    drawCO();
    $('#bs-grafCO').off(); //to remove the binded event after initial rendering
});
$('#bs-grafCons').on("shown.bs.tab", function () {
    drawConsumo();
    $('#bs-grafCons').off(); //to remove the binded event after initial rendering
});

function changeSel() {
    var IdCPD = encodeURIComponent($("#CPD_DD").val());
    var IdPeriodicidad = encodeURIComponent($("#Periodicidad_DD").val());
    var sStartTime = encodeURIComponent($("#StartDate").val());
    var sEndTime = encodeURIComponent($("#EndDate").val());
    //document.location.href = "Index?selectedID=" + IdCPD + "&selectedPeriodicidad=" + IdPeriodicidad + "&";
    window.location.href = "Index?selectedID=" + IdCPD +
                            "&selectedPeriodicidad=" + IdPeriodicidad +
                            "&FechaInicio=" + sStartTime +
                            "&FechaFin=" + sEndTime + "&";
}

$("#CPD_DD").change(changeSel);
$("#Periodicidad_DD").change(changeSel);

/*$("#StartDate").blur(changeSel);
$("#EndDate").blur(changeSel);*/

$('#btnRe').click(changeSel);

$(function () {
    $('#DateTimPickerStart').datetimepicker({
        locale: 'es'
    });
    $('#DateTimPickerEnd').datetimepicker({
        useCurrent: false, locale: 'es' //Important! See issue #1075
    });
    $("#DateTimPickerStart").on("dp.changed", function (e) {
        $('#DateTimPickerEnd').data("DateTimePicker").minDate(e.date);
    });
    $("#DateTimPickerEnd").on("dp.changed", function (e) {
        $('#DateTimPickerStart').data("DateTimePicker").maxDate(e.date);
    });
});