// Init once (ideally on DOM ready)
$(function () {
    // Custom methods
    $.validator.addMethod("valueNotEquals", function (value, element, arg) {
        return arg !== value;
    }, "Please select a value.");

    $.validator.addMethod("pwstrong", function (value, element) {
        if (this.optional(element)) return true;
        // ≥8 chars, 1 upper, 1 lower, 1 digit, 1 special
        return /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/.test(value);
    }, "Must be at least 8 characters and include uppercase letters, lowercase letters, numbers, and special characters.");
    $.validator.addMethod("uniqueFullName", function (value, element) {
        if (!value.trim()) return true; // ignore empty values

        var $form = $(element).closest('#kt_repeater_Form');
        var allFullNames = $form.find('input[name*="FullName"]');
        var valueLower = value.trim().toLowerCase();
        var duplicates = 0;

        allFullNames.each(function () {
            if ($(this).val().trim().toLowerCase() === valueLower && $(this).val().trim() !== "") {
                duplicates++;
            }
        });

        // Valid if duplicates count <= 1
        return duplicates <= 1;
    }, "Full Name must be unique.");

    // Class-based rules    
    $.validator.addClassRules({
        RequiredElement: { required: true },
        RequiredEmail: { required: true, email: true },
        EmailOnly: { email: true },
        RequiredNumber: { required: true, number: true },
        NumberOnly: { number: true },
        RequiredSelect: { valueNotEquals: "-1" },
        PasswordStrong: { required: true, pwstrong: true },
        FullNameUnique: { required: true, uniqueFullName: true } // new rule

    });
});

// Function to reapply equalTo rule dynamically
function ApplyConfirmMatchRules(context) {
    $(context).find(".ConfirmMatch").each(function () {
        var target = $(this).data("equalto");
        $(this).rules("add", {
            required: true,
            equalTo: target,
            messages: { equalTo: "Passwords do not match." }
        });
    });
}

// Main validate wrapper
function Validate(form) {
    if (!form) {
        form = "form1";
    }

    // initialize validator (if not already initialized)
    var $form = $("#" + form);
    if (!$form.data("validator")) {
        $form.validate({
            errorElement: "span",
            errorClass: "text-danger",
            ignore: ":hidden",
            highlight: function (el) {
                $(el).addClass("is-invalid");
            },
            unhighlight: function (el) {
                $(el).removeClass("is-invalid");
            }
        });
    }

    // 🧩 Reapply confirm match rules before validation runs
    ApplyConfirmMatchRules("#" + form);

    // run validation
    var result = $form.valid();
    if (!result) {
        ShowAlert('error', "Error", 'Please fill the fields in red correctly!');
    }
    return result;
}
