var DateTimeFormat = $("#hfDateTimeFormat").val();
var DateFormat = $("#hfDateFormat").val();
var calenderFormatDate = 'MM/dd/yyyy'
var calenderFormatTimeDate = 'MM/dd/yyyy hh:mm t'

$(document).ready(function () {
    TimePicker();
    InitDatePickers();
});

function InitDatePickers() {

    // Loop through each element with class 'DateCustom'
    $(".DateCustom").each(function () {
        const element = this; // 'this' is the DOM element
        const enableClock = $(element).data("enableclock") == undefined ? false : true;
        var format = '';
        if (enableClock) {
            format = calenderFormatTimeDate
        }
        else {
            format = calenderFormatDate
        }
        const options = {
            localization: { format: format },
            display: {
                components: {
                    calendar: true,
                    clock: enableClock
                }
            }
        };
        if ($(element).data("maxtoday") == true) {
            options.restrictions = {
                maxDate: new Date()
            };
        }
        const picker = new tempusDominus.TempusDominus(element, options);

        // From / To restriction
        if (element.hasAttribute("data-fromrestriction")) {

            picker.subscribe(tempusDominus.Namespace.events.change, function (e) {

                const toElement = document.querySelector("[data-torestriction]");
                if (!toElement || !toElement._td) return;

                const toPicker = toElement._td;
                toPicker.updateOptions({
                    restrictions: {
                        minDate: e.date
                    }
                });

            });

        }
        if (element.hasAttribute("data-fromrestriction-Attendance")) {

            picker.subscribe(tempusDominus.Namespace.events.change, function (e) {

                const toElement = document.querySelector("[data-torestriction]");
                if (!toElement || !toElement._td) return;

                const toPicker = toElement._td;
                toPicker.updateOptions({
                    restrictions: {
                        minDate: e.date
                    }
                });
                const toDate = toPicker.dates.lastPicked;
                if (!toDate || toDate < e.date) {
                    toPicker.dates.setValue(e.date); // ✅ set same value
                }

            });

        }

        if (element.hasAttribute("data-torestriction")) {

            picker.subscribe(tempusDominus.Namespace.events.change, function (e) {

                const fromElement = document.querySelector("[data-fromrestriction]");
                if (!fromElement || !fromElement._td) return;

                const fromPicker = fromElement._td;
                fromPicker.updateOptions({
                    restrictions: {
                        maxDate: e.date
                    }
                });

            });

        }
         // ============================================
        // 🔥 Current Month + Restrictions (FIXED)
        // ============================================

        let today = new Date();
        let firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
        let lastDay = today;

        // respect max today
        if ($(element).data("maxtoday") == true && lastDay > today) {
            lastDay = today;
        }

        let tdFirst = tempusDominus.DateTime.convert(firstDay);
        let tdLast = tempusDominus.DateTime.convert(lastDay);

     
        // store picker instance
        element._td = picker;
    });

}
function FormatDate(date) {
    return String(date.getMonth() + 1).padStart(2, '0') + '/' +
           String(date.getDate()).padStart(2, '0') + '/' +
           date.getFullYear();
}
function FormatTimeForServer(timeStr) {
    // Convert "05:39 AM" or "5:39 PM" -> "HH:mm:ss"
    const d = new Date("1970-01-01 " + timeStr); // JS parses AM/PM
    const hours = d.getHours().toString().padStart(2, "0");
    const minutes = d.getMinutes().toString().padStart(2, "0");
    return `${hours}:${minutes}:00`;
}
function FormatTimeAMPM(time) {

    if (!time) return "";

    var parts = time.split(':');
    var hours = parseInt(parts[0]);
    var minutes = parts[1];

    var ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12;

    return hours + ':' + minutes + ' ' + ampm;
}
function TimePicker() {

    const timeElements = $(".TimeCustom");
    const dateElements = $(".DD");


    timeElements.each(function () {

        const element = this;

        let timePicker = new tempusDominus.TempusDominus(element, {
            display: {
                components: {
                    calendar: false,
                    date: false,
                    month: false,
                    year: false,
                    decades: false,
                    hours: true,
                    minutes: true,
                    seconds: false
                }
            },
            localization: {
                hourCycle: "h12",
                format: "hh:mm T"
            },
            restrictions: {}
        });
        if (dateElements != undefined)
            if (dateElements.length > 0) {

                const updateTimeRestrictions = () => {

                    const todayStr = moment().format("MM/DD/YYYY");
                    const selectedDate = dateElement.val();

                    let currentTime = timePicker.dates.lastPicked || new tempusDominus.DateTime();

                    if (selectedDate === todayStr) {

                        const now = new tempusDominus.DateTime();
                        now.setSeconds(0);

                        timePicker.updateOptions({
                            restrictions: { minDate: now }
                        });

                    } else {

                        timePicker.updateOptions({
                            restrictions: {}
                        });

                    }

                    timePicker.dates.setValue(currentTime);
                };

                // Initial check
                updateTimeRestrictions();

                // When date changes
                dateElement.on("change", updateTimeRestrictions);
            }

    });

}
function FormatDateMMDDYYYY(dateStr) {
    if (!dateStr) return "";
    var date = new Date(dateStr); // parse backend date
    var month = (date.getMonth() + 1).toString().padStart(2, '0');
    var day = date.getDate().toString().padStart(2, '0');
    var year = date.getFullYear();
    return month + '/' + day + '/' + year;
}