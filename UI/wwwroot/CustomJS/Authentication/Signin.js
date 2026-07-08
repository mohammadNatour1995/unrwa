
function getQueryStringParam(name) {
    var params = new URLSearchParams(window.location.search);
    return params.get(name);
}
function ShowLoader() {
    $('.loader-container').show();
}
function HideLoader() {
    $('.loader-container').hide();
}
function Signin() {
    if (!$("#txtPassword").is(":visible")) {
        $("#txtPassword").val("");
    }
    if (Validate()) {
        ShowLoader()
        var data = {
            Email: $("#txtEmail").val(),
            Password: $('#txtPassword').val()
        };
        $.ajax({
            type: "POST",
            url: "/Account/Login",
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",
            success: function (result) {
                switch (result.Header.Status) {
                    //Success
                    case 1:
                        var returnUrl = getQueryStringParam("returnUrl");
                        if (returnUrl && returnUrl.startsWith("/") && !returnUrl.startsWith("//")) {
                            window.location.href = returnUrl;
                        } else {
                            window.location.href = "/Home/Dashboard";
                        }
                        break;
                    //CusstomError
                    case 10:
                        ShowAlert(result.Header.Message.MessageCode, result.Header.Message.MessageCode, result.Header.Message.MessageDesc);
                        ClearForm();
                        break;
                    //Unauthorized
                    case 4:
                        ShowAlert("error", "Error", "Invalid email or password!");
                        ClearForm();
                        break;
                    
                    //Redirect
                    case 8:
                        window.location.href = result.Header.Message.MessageDesc;
                        break;
                    default:
                        ShowAlert("error", "Erorr", "Failed to login, please try again later!");
                }
                HideLoader();
            },
            complete: function () {
                HideLoader();
            },
            error: function (xhr) {
                HideLoader();
                ShowAlert("error", "Error", "Oops! something went wrong!");
            }
        });
    }
}
