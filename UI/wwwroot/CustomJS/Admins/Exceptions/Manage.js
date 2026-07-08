$(document).ready(function () {
    GetData();
});

function GetData() {
    DataGrid.init({
        columnsLabels: ['Message', 'Level', 'TimeStamp', 'Function Name', 'Exception', 'Actions'],
        ajaxUrl: '/Admins/Exceptions/ReadAllPagination',
        mapOrderBy: { 0: 'Message', 1: 'Level', 2: 'TimeStamp', 3: 'FunctionName', 4: 'Exception' },
        rowId: 'Id',
        buildFilters: () => ({
            message: $('#txtMessage').val() || null,
            functionName: $('#txtFunctionName').val() || null,
        }),
        columns: [
            { data: 'Message' },
            { data: 'Level' },
            { data: 'TimeStamp', type: 'date' },
            { data: 'FunctionName' },
            { data: 'Exception' },
            {
                data: null, orderable: false,
                render: (d, t, row) => {
                    const actions = [{ key: 'view', text: 'View Exception' }];
                    return DataGrid.menu(row, actions);
                }
            }
        ]
    });

    DataGrid.delegate('#tblData', {
        view: ({ id }) => { ViewException(id); },
    });
}
function GetUsers() {
    var data = {};
    var successCallbackFunction = function (result) {

        if (result.Header.Status != 1) {
            ShowAlert("error", "Error", "Failed to load users, please try again later!");
            return;
        }

        var $ddl = $('#ddlContactUser');

        $ddl.empty().append('<option value="-1">Please Select</option>');

        result.Data.forEach(function (user) {
            $ddl.append(
                $('<option/>', {
                    value: user.UserID, text: user.Name, 'data-Department': user.DepartmentID, 'data-Unit': user.UnitID, 'data-OfficeNumber': user.OfficeNumber, 'data-FloorNumber': user.FloorNumber
                })
            );

        });
    };
    CallAjaxMethod("POST", "/Admins/Users/ReadAll", data, successCallbackFunction, "Oops something went wrong!")
}
function ViewException(id) {
 
    GetExceptionByID(id)
}

function FormatJson(value) {
    if (!value) return "";

    try {
        const jsonObj = typeof value === "string"
            ? JSON.parse(value)
            : value;

        return JSON.stringify(jsonObj, null, 4);
    }
    catch {
        return value;
    }
}

function GetExceptionByID(exceptionId) {

    const modal = new bootstrap.Modal(
        document.getElementById('logDetailsModal')
    );
    const data = {
        
        id: exceptionId
    };

    const successCallbackFunction = function (result) {
        switch (result.Header.Status) {
            case 1:
                let logEventProps = {};
                try {
                    const logEvent = JSON.parse(result.Data.LogEvent || '{}');
                    logEventProps = logEvent.Properties || {};
                } catch {}

                $('#tdId').text(result.Data.Id);
                $('#tdMessage').text(result.Data.Message);
                $('#tdLevel')
                    .text(result.Data.Level)
                    .removeClass()
                    .addClass(`badge ${GetLevelClass(result.Data.Level)}`);
                $('#tdTimestamp').text(result.Data.TimeStamp);
                $('#tdUserName').text(result.Data.UserName ?? '');
                $('#tdRequestPath').text(result.Data.RequestPath ?? '');
                $('#tdFunctionName').text(result.Data.FunctionName ?? '');
                $('#tdParameters').html(
                    `<code>${FormatJson(result.Data.Parameters)}</code>`
                );
                $('#tdException').html(
                    `<code>${result.Data.Exception ?? ''}</code>`
                );
               
                modal.show();
                break;
            default:

                ShowAlert(
                    "error",
                    "Error",
                    "Failed to load exception details!"
                );

                break;
        }
    };

    CallAjaxMethod(
        "POST",
        "/Admins/Exceptions/Find",
        data,
        successCallbackFunction,
        "Oops something went wrong!"
    );
}

function GetLevelClass(level) {

    switch ((level || '').toLowerCase()) {

        case 'error':
            return 'badge badge-danger';

        case 'warning':
            return 'badge badge-warning';

        case 'information':
            return 'badge badge-info';

        case 'debug':
            return 'badge badge-primary';

        default:
            return 'badge badge-secondary';
    }
}
