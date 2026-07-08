$(document).ready(function () {
    LoadRoles();
    GetData();
});

var isEditMode = false;

function GetData() {
    DataGrid.init({
        columnsLabels: ['User Name', 'Email', 'Phone Number', 'Full Name', 'Role', 'Active', 'Actions'],
        ajaxUrl: '/Admins/Users/ReadAllPagination',
        mapOrderBy: { 0: 'UserName', 1: 'Email', 2: 'PhoneNumber', 3: 'FullName', 4: 'Role', 5: 'IsActive' },
        rowId: 'Id',
        buildFilters: () => ({
            userName: $('#txtUserName').val() || null,
            email: $('#txtEmail').val() || null,
        }),
        columns: [
            { data: 'UserName' },
            { data: 'Email' },
            { data: 'PhoneNumber' },
            { data: 'FullName' },
            { data: 'Role' },
            {
                data: 'IsActive',
                render: (v) => v
                    ? '<span class="badge badge-success">Active</span>'
                    : '<span class="badge badge-secondary">Inactive</span>'
            },
            {
                data: null, orderable: false,
                render: (d, t, row) => {
                    const actions = [
                        { key: 'edit', text: 'Edit' },
                        { key: 'delete', text: 'Delete' }
                    ];
                    return DataGrid.menu(row, actions);
                }
            }
        ]
    });

    DataGrid.delegate('#tblData', {
        edit: ({ id }) => { OpenEditModal(id); },
        delete: ({ id }) => { DeleteUser(id); },
    });
}

function LoadRoles() {
    const successCallbackFunction = function (result) {
        if (result.Header.Status != 1) return;

        const $ddl = $('#ddlFormRole');
        $ddl.empty().append('<option value="-1">Please Select</option>');

        (result.Data || []).forEach(function (role) {
            $ddl.append($('<option/>', { value: role, text: role }));
        });
    };

    CallAjaxMethod('POST', '/Admins/Users/GetRoles', {}, successCallbackFunction, 'Oops something went wrong!');
}

function OpenAddModal() {
    isEditMode = false;
    ClearForm();
    $('#hfUserId').val('');
    $('#userModalTitle').text('Add User');
    $('#divFormPassword').show();
    $('#divFormIsActive').hide();

    const modal = new bootstrap.Modal(document.getElementById('userModal'));
    modal.show();
}

function OpenEditModal(id) {
    const successCallbackFunction = function (result) {
        if (result.Header.Status != 1) {
            ShowAlert('error', 'Error', 'Failed to load user, please try again later!');
            return;
        }

        isEditMode = true;
        ClearForm();

        const data = result.Data;
        $('#hfUserId').val(data.Id);
        $('#txtFormUserName').val(data.UserName);
        $('#txtFormFullName').val(data.FullName);
        $('#txtFormEmail').val(data.Email);
        $('#txtFormPhoneNumber').val(data.PhoneNumber);
        $('#ddlFormRole').val(data.Role).trigger('change');
        $('#chkFormIsActive').prop('checked', !!data.IsActive);

        $('#userModalTitle').text('Edit User');
        $('#divFormPassword').hide();
        $('#divFormIsActive').show();

        const modal = new bootstrap.Modal(document.getElementById('userModal'));
        modal.show();
    };

    CallAjaxMethod('POST', '/Admins/Users/Find', { id: id }, successCallbackFunction, 'Oops something went wrong!');
}

function DeleteUser(id) {
    Swal.fire({
        title: 'Are you sure?',
        text: 'This user will be permanently deleted.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it',
        buttonsStyling: false,
        customClass: {
            confirmButton: 'btn btn-danger',
            cancelButton: 'btn btn-light'
        }
    }).then((res) => {
        if (!res.isConfirmed) return;

        const successCallbackFunction = function (result) {
            if (result.Header.Status != 1) {
                ShowAlert('error', 'Error', result.Header.Message || 'Failed to delete user!');
                return;
            }
            ShowAlert('success', 'Deleted', 'User deleted successfully.');
            DataGrid.reload('tblData');
        };

        CallAjaxMethod('POST', '/Admins/Users/Delete', { id: id }, successCallbackFunction, 'Oops something went wrong!');
    });
}

$('#btnAddUser').on('click', function () {
    OpenAddModal();
});

$('#btnSaveUser').on('click', function () {
    if (!Validate('frmUser')) return;

    const dto = {
        Id: $('#hfUserId').val() || null,
        UserName: $('#txtFormUserName').val(),
        FullName: $('#txtFormFullName').val(),
        Email: $('#txtFormEmail').val(),
        PhoneNumber: $('#txtFormPhoneNumber').val(),
        Role: $('#ddlFormRole').val(),
        IsActive: isEditMode ? $('#chkFormIsActive').is(':checked') : true,
        Password: isEditMode ? null : $('#txtFormPassword').val()
    };

    const url = isEditMode ? '/Admins/Users/Update' : '/Admins/Users/Add';

    const successCallbackFunction = function (result) {
        if (result.Header.Status != 1) {
            ShowAlert('error', 'Error', result.Header.Message || 'Failed to save user!');
            return;
        }

        bootstrap.Modal.getInstance(document.getElementById('userModal'))?.hide();
        ShowAlert('success', 'Saved', 'User saved successfully.');
        DataGrid.reload('tblData');
    };

    CallAjaxMethod('POST', url, dto, successCallbackFunction, 'Oops something went wrong!');
});
