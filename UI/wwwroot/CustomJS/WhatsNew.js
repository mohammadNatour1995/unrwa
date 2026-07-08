$(document).ready(function () {
    GetWhatsNew()
});
function GetWhatsNew() {
    var data = {};

    var successCallbackFunction = function (result) {
        if (result.Header.Status != 1) {
            ShowAlert("error", "Error", "Failed to load updates, please try again later!");
            return;
        }

        var $container = $('#whatsNewContainer');
        $container.empty();

        result.Data.forEach(function (item) {
            var html = `
                <div class="col-md-12 mb-5">
                    <div class="card shadow-sm h-100">
                        <div class="card-body">
                            <div class="d-flex justify-content-between align-items-start mb-2">
                                <h5 class="card-title text-gray-900 fw-bold">${item.Title}</h5>
                                <span class="badge badge-light-primary fw-bold">v${item.Version}</span>
                            </div>
                            <p class="text-muted fs-7 mb-2">
                                <i class="flaticon2-calendar-2"></i>
                                Release Date: ${new Date(item.ReleaseDate).toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' })}
                            </p>
                            <p class="card-text text-gray-700 fs-6">${item.Features}</p>
                        </div>
                    </div>
                </div>
            `;
            $container.append(html);
        });
    };

    CallAjaxMethod("POST", "/Admins/WhatsNew/GetAll", data, successCallbackFunction, "Oops something went wrong!");
}
