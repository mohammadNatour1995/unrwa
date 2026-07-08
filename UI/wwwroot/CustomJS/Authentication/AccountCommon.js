
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
function ShowLoader() {
    $('.loader-container').show();
}
function HideLoader() {
    $('.loader-container').hide();
}