"use strict";

/**
 * Initialize Quill editors on elements with the class "kt-quill-editor"
 * Automatically reads optional data-height attribute for custom height
 */
function InitQuillEditors() {
    const editors = document.querySelectorAll(".kt-quill-editor");

    if (!editors.length) return;

    editors.forEach((el) => {
        // Skip if already initialized
        if (el.__quillInstance) return;

        const height = el.dataset.height || 250;
        el.style.height = height + "px";

        // Initialize Quill
        const quill = new Quill(el, {
            modules: {
                toolbar: [
                    [{ header: [1, 2, 3, false] }],
                    ["bold", "italic", "underline"],
                    [{ list: "ordered" }, { list: "bullet" }],
                    ["link", "image"],
                    [{ align: [] }],
                    [{ color: [] }, { background: [] }]
                ]
            },
            placeholder: "Type your text here...",
            theme: "snow"
        });

        // Store instance on element for later use
        el.__quillInstance = quill;
    });
}

// Auto-init all editors on DOMContentLoaded
document.addEventListener("DOMContentLoaded", function () {
    InitQuillEditors();
});
function SetQuillContentById(id, htmlContent) {
    var el = document.getElementById(id);
    if (!el || !el.__quillInstance) return;

    el.__quillInstance.root.innerHTML = htmlContent;
}
