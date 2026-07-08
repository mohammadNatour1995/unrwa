
$(document).ready(function () {
    SetMenu();
});
function ArabicButton() {
    ShowAlert("info", "info", "Arabic language coming soon");
}
function SetMenu() {
    const currentUrl = (window.location.pathname || "").toLowerCase();
    const segments = currentUrl.split('/').filter(Boolean);

    const currentArea = segments[0] || "dashboard";
    const currentController = segments[1] || "";

    let bestMatch = null;
    let bestScore = -1;

    $("#kt_aside_menu_wrapper .menu-link").removeClass("active");
    $("#kt_aside_menu_wrapper .menu-item").removeClass("here show");

    $("#kt_aside_menu_wrapper .menu-link").each(function () {
        const $link = $(this);
        const href = ($link.attr("href") || "").toLowerCase();
        const menuArea = ($link.data("menu-area") || "").toString().toLowerCase();

        if (!href) return;

        const linkParts = href.split('/').filter(Boolean);
        const linkArea = linkParts[0] || "";
        const linkController = linkParts[1] || "";

        let score = 0;

        // ✅ 1. exact match
        if (href === currentUrl) {
            score = 3;
        }
        // ✅ 2. area + controller (fixes Admin submenu issue)
        else if (menuArea === currentArea && linkController === currentController) {
            score = 2;
        }
        // ✅ 3. area only (fixes Employees vs Attendance issue)
        else if (menuArea === currentArea || linkArea === currentArea) {
            score = 1;
        }

        if (score > bestScore) {
            bestMatch = $link;
            bestScore = score;
        }
    });

    if (bestMatch) {
        bestMatch.addClass("active");
        bestMatch.closest(".menu-item").addClass("here show");
        bestMatch.closest(".menu-sub").parent(".menu-item.menu-accordion").addClass("here show");
    }
}
function SetCookie(cname, cvalue, exdays) {
    const d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    let expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}
function ClearForm() {
    $('input:text, input:password, input:file, textarea').val('');
    $('select').each(function () {
        var firstValue = $(this).find('option:first').val();
        $(this).val(firstValue).trigger('change');
    });
    $("input:checkbox").prop("checked", false);
    $('.select2').val(null).trigger('change');
    ResetAllQuillEditors();
    $('#kt_repeater_Form').find('[data-repeater-item]').not(':first').remove();
}
function ClearModal() {
    $("#divMainModal_Header_Title").text("Titel");
    $("#divMainModal_Body").empty();
    $("#divMainModal_Footer").empty();
    $("#divMainModal").modal('hide');
}
function ShowLoader() {
    $('.loader-container').show();
}
function HideLoader() {
    $('.loader-container').hide();
}
function CallAjaxMethod(type, url, data, successCallbackFunction, errorCallbackMessage, async = true, customCallbackFunction) {
    ShowLoader();
    $.ajax({
        type: type,
        url: url,
        async: async,
        headers: {
            'Authorization': 'Bearer ' + GetCookie("AccessToken")
        },
        data: data,
        contentType: "application/x-www-form-urlencoded; charset=UTF-8",
        success: function (result) {
            if (result) {
                switch (result.Header.Status) {
                    //CusstomError
                    //case 4:
                    //    if (customCallbackFunction) {
                    //        customCallbackFunction(result)
                    //    }
                    //    ShowAlert(result.Header.Message.MessageCode, result.Header.Message.MessageCode, result.Header.Message.MessageDesc);
                    //    break;
                    //Unauthorized
                    case 4:
                        window.location.href = "/Account/Signin?returnUrl=" + encodeURIComponent(window.location.pathname + window.location.search);
                        break;
                    //Forbidden
                    //case 6:
                    //    window.location.href = "/Home/Dashboard";
                    //    break;
                    //case 7:
                    //    ShowAlert("error", "Error", "Validation error");
                    //    break;
                    default:
                        successCallbackFunction(result);
                }
            }
            else {
                ShowAlert("error", "Error", errorCallbackMessage);
            }
            HideLoader();
        },
        complete: function () {
            HideLoader();
        },
        error: function (xhr) {
            HideLoader();
            if (xhr.status === 401) {
                debugger;
                AuthenticateUser(type, url, data, successCallbackFunction, errorCallbackMessage, async = true, customCallbackFunction);
            }
            else {
                ShowAlert("error", "Error", "Oops! something went wrong!");
            }
        }
    });
}
function AuthenticateUser(type, url, data, successCallbackFunction, errorCallbackMessage, async = true, customCallbackFunction) {
    var successCallbackFunction = function (result) {

        if (result.Header.Status == 0) {
            CallAjaxMethod(type, url, data, successCallbackFunction, errorCallbackMessage, async, customCallbackFunction)
        }
        else {
            window.location.href = "/Account/Signin?returnUrl=" + encodeURIComponent(window.location.pathname + window.location.search);
        }
    };

    CallAjaxMethod("POST", "/Signin/AuthenticateUser", null, successCallbackFunction, "Oops something went wrong!")
}
function ShowAlert(type, title, message) {
    type = type.toLowerCase();
    HideLoader();
    switch (type) {
        case "success":
            Swal.fire({
                title: title,
                text: message,
                icon: type,
                buttonsStyling: false,
                confirmButtonText: "Ok",
                customClass: {
                    confirmButton: "btn btn-success"
                }
            });
            break;
        case "info":
            Swal.fire({
                title: title,
                text: message,
                icon: type,
                buttonsStyling: false,
                confirmButtonText: "Ok",
                customClass: {
                    confirmButton: "btn btn-info"
                },
            });
            break;
        case "warning":
            Swal.fire({
                title: title,
                text: message,
                icon: type,
                buttonsStyling: false,
                confirmButtonText: "Ok",
                customClass: {
                    confirmButton: "btn btn-warning"
                },
            });
            break;
        case "error":
            Swal.fire({
                title: title,
                text: message == null ? "Oops! something went wrong!" : message,
                icon: type,
                buttonsStyling: false,
                confirmButtonText: "Ok",
                customClass: {
                    confirmButton: "btn btn-danger"
                },
            });
            break;
        case "question":
            Swal.fire({
                title: title,
                text: message,
                icon: type,
                buttonsStyling: false,
                confirmButtonText: "Ok",
            });
            break;
        default:
    }
}
function GetCookie(cname) {
    let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}
// ✅ Open the menu when the "Actions" button is clicked
$(document).on('click', '.action-trigger', function (e) {
    e.preventDefault();

    // Ensure KTMenu is initialized
    if (typeof KTMenu === "undefined") {
        console.error("KTMenu is not loaded.");
        return;
    }

    // Get the next sibling menu
    var menu = this.nextElementSibling;
    if (!menu) return;

    // Get or create instance
    var instance = KTMenu.getInstance(menu);
    if (!instance) {
        if (KTMenu.createInstances) {
            KTMenu.createInstances();
        } else {
            instance = new KTMenu(menu);
        }
    }

    // Open menu
    instance = KTMenu.getInstance(menu);
    if (instance && typeof instance.show === "function") {
        instance.show();
    } else {
        //console.warn("Unable to open KTMenu — no valid instance found.");
    }
});

//Filter Search and Clear
$('#btnFilter').on('click', function (e) {
    e.preventDefault();           // if the button is inside a form
    DataGrid.reload('tblData', true); // true => reset paging to page 1
});
$('#btnClear').on('click', function (e) {
    e.preventDefault();           // if the button is inside a form
    ClearForm();
    DataGrid.reload('tblData', true); // true => reset paging to page 1
});
function ResetAllQuillEditors() {
    const editors = document.querySelectorAll(".kt-quill-editor");

    editors.forEach(div => {
        if (div.__quillInstance) {
            div.__quillInstance.setContents([]); // Clears
        }
    });
}
