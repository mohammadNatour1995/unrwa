/* table.js - minimal, generic DataGrid for jQuery DataTables + Metronic
   Requirements: jQuery, DataTables (+ Buttons if exporting), Metronic KTMenu (optional)
*/
; (() => {
    // -------- Config defaults (override per table as needed) --------
    const DEFAULTS = {
        id: 'tblData',
        containerSelector: '#divTable',
        enableExport: true,                   // turn on to show Copy/CSV/Excel/PDF/Print
        exportTitle: document.title || 'Export',
        exportExcludeSelector: ':last-child',  // avoid exporting Actions column
        defaultSort: 'CreatedDate DESC',       // <-- NEW: server-side fallback sort
    }

    // -------- Minimal helpers (only what we truly need) -------------
    const safeCssId = (val) => (val == null ? '' : String(val).replace(/[^A-Za-z0-9_\-:.]/g, '_'))

    const w = window;
    // Default Datetime format (uses window.DateTimeFormat or #hfDateTimeFormat)
    const displayOrDash = (val) => (val == null || val === '' ? '-' : val);

    // ✅ NEW: normalize text (supports Arabic) + find Actions column index
    const normText = (s) => (s ?? '')
        .toString()
        .trim()
        .toLowerCase()
        .replace(/\s+/g, ' ')
        .replace(/[^\p{L}\p{N}\s]/gu, '')

    const findActionsIndex = (opts, dtCols) => {
        const labels = opts.columnsLabels || []
        const keys = new Set(['Actions', 'action', 'اجراءات', 'الإجراءات', 'الاجراءات'].map(normText))

        // 1) by header label
        let idx = labels.findIndex(h => keys.has(normText(h)))
        if (idx >= 0) return idx

        // 2) by col config hint (optional)
        idx = (dtCols || []).findIndex(c => {
            const hay = normText([c?.data, c?.name, c?.className].filter(Boolean).join(' '))
            return hay.includes('action') || hay.includes(normText('اجراءات')) || hay.includes(normText('الإجراءات'))
        })
        if (idx >= 0) return idx

        // 3) no actions column found
        return -1
    }

    // Add Sumary
    const ensureTable = (opts) => {
        const $container = $(opts.containerSelector);
        if (!$container.length) throw new Error(`Container "${opts.containerSelector}" not found.`);

        let $table = $container.find(`#${opts.id}`);
        if (!$table.length) {
            $table = $(`<table id="${opts.id}" class="table table-hover table-rounded border gy-7 gs-7" style="width:100%"></table>`);
            $container.empty().append($table);
        } else {
            $table.addClass("table table-hover table-rounded border gy-7 gs-7");
        }

        // Build/refresh header from labels
        const head = `<thead><tr class="fw-semibold fs-6 text-gray-800 border-bottom border-gray-200">` +
            opts.columnsLabels.map(h => `<th>${h}</th>`).join('') +
            `</tr></thead>`;

        if (!$table.find('thead').length || $table.find('thead th').length !== opts.columnsLabels.length) {
            $table.find('thead').remove();
            $table.prepend(head);
        } else {
            $table.find('thead tr').addClass("fw-semibold fs-6 text-gray-800 border-bottom border-gray-200");
        }

        if (!$table.find('tbody').length) $table.append('<tbody></tbody>');
        return $table;
    };

    // -------- Public DataGrid API -----------------------------------
    const DataGrid = {
        _tables: new Map(),

        init(userOpts) {
            // Merge defaults
            const opts = { ...DEFAULTS, ...(userOpts || {}) }

            // Required
            const required = ['columnsLabels', 'ajaxUrl', 'mapOrderBy', 'rowId', 'buildFilters', 'columns']
            const miss = required.filter(k => opts[k] == null)
            if (miss.length) throw new Error(`DataGrid.init: missing option(s): ${miss.join(', ')}`)

            const $table = ensureTable(opts)
            if ($.fn.DataTable.isDataTable($table)) {
                $table.DataTable().destroy(true)
                ensureTable(opts)
            }

            // Build columns (allow simple type: 'date' shortcut)
            const dtCols = opts.columns.map(c => {
                if (c.type === 'date' && !c.render) {
                    // use defaultDateFormatter (moment-based)
                    return {
                        ...c,
                        render: (v) => {
                            if (v == null || v === '') return '-';

                            const formatted = moment(v).format(
                                w.DateTimeFormat || $("#hfDateTimeFormat").val()
                            );

                            return formatted || '-';
                        }
                    }
                }
                if (c.type === 'dateOnly' && !c.render) {
                    // use defaultDateFormatter (moment-based)
                    return {
                        ...c,
                        render: (v) => {
                            if (v == null || v === '') return '-';

                            const formatted = moment(v).format(
                                w.DateFormat || $("#hfDateFormat").val()
                            );

                            return formatted || '-';
                        }
                    }
                }
                // TIME FORMAT
                if (c.type === 'time' && !c.render) {
                    return {
                        ...c,
                        render: (v) => {
                            if (v == null || v === '') return '-';

                            return moment(v, "HH:mm:ss").format(
                                w.TimeFormat || $("#hfTimeFormat").val()
                            ) || '-';
                        }
                    }
                }
                // If no render function, show "-" for null/empty
                if (!c.render) {
                    return { ...c, render: (v) => displayOrDash(v) }
                }
                return c
            })

            // ✅ NEW: force Actions column to always show on small screens
            const actionIdx = findActionsIndex(opts, dtCols)

            // Add Responsive class "all" => always visible
            if (dtCols[actionIdx]) {
                const existing = (dtCols[actionIdx].className || "")
                dtCols[actionIdx] = { ...dtCols[actionIdx], className: (existing + " all").trim() }
            }

            // Inline ajax (uses your CallAjaxMethod; no extra helper)
            const ajax = (req, callback /*, settings */) => {
                const hasOrder = Array.isArray(req.order) && req.order.length > 0
                const idx = hasOrder ? req.order[0].column : null
                const dir = hasOrder ? String(req.order[0].dir || 'DESC').toUpperCase() : 'DESC'
                const mapped = (idx != null) ? opts.mapOrderBy[idx] : null
                const orderBy = (hasOrder && mapped)
                    ? `${mapped} ${dir}`
                    : (opts.defaultSort || 'DateCreated DESC')

                const filters = (typeof opts.buildFilters === 'function' ? opts.buildFilters() : {}) || {}
                const extra = (typeof opts.extraRequest === 'function' ? opts.extraRequest() : {}) || {}

                const payload = Object.assign({}, filters, {
                    pageNumber: (req.start / req.length) + 1,
                    pageSize: req.length,
                    orderBy: orderBy
                }, extra)

                // IMPORTANT: assumes your API returns ReturnResponse with Data[] and TotalRecords on [0]
                CallAjaxMethod('POST', opts.ajaxUrl, payload, (res) => {
                    if (res?.Header?.Status == 1) {
                       
                        const rows = res.Data.Items || []
                        const total = res.Data.TotalCount || 0
                        callback({ draw: req.draw, recordsTotal: total, recordsFiltered: total, data: rows })
                    }
                    else if (res?.Header?.Status === 2) {
                        ShowAlert("error", "Error", "Failed to load data, please try again later!");
                        callback({ draw: req.draw, recordsTotal: 0, recordsFiltered: 0, data: [] })
                    }
                    else {
                        // graceful fallback
                        callback({ draw: req.draw, recordsTotal: 0, recordsFiltered: 0, data: [] })
                    }
                }, 'Oops! something went wrong!')
            };

            // Single helper to derive initial order from defaultSort (robust to array/object mapOrderBy)
            const getInitialOrder = () => {
                const [fieldRaw, dirRaw] = String(opts.defaultSort || 'DateCreated DESC').trim().split(/\s+/);
                const field = (fieldRaw || '').toLowerCase();
                const dir = (dirRaw || 'DESC').toLowerCase();

                // Normalize mapOrderBy to an array in column order
                const mapArr = Array.isArray(opts.mapOrderBy)
                    ? opts.mapOrderBy
                    : (opts.mapOrderBy && typeof opts.mapOrderBy === 'object'
                        ? Object.keys(opts.mapOrderBy)
                            .sort((a, b) => (+a) - (+b))
                            .map(k => opts.mapOrderBy[k])
                        : []);

                // Find index whose mapped field matches the field token (compare first token of mapping)
                const idx = mapArr.findIndex(m => {
                    const firstToken = String(m || '').trim().split(/\s+/)[0].toLowerCase();
                    return firstToken === field;
                });

                return idx >= 0 ? [[idx, dir]] : [];
            };

            const dt = $table.DataTable({
                processing: true,
                serverSide: true,
                searching: false,
                responsive: true,
                lengthChange: true,
                ordering: true,
                order: getInitialOrder(),
                ajax,

                // ✅ NEW: make Actions column highest priority + collapse others first
                columnDefs: [
                    { targets: actionIdx, responsivePriority: 1, orderable: actionIdx !== (dtCols.length - 1) }, // actions column
                    { targets: '_all', responsivePriority: 100, orderSequence: ['desc', 'asc'] }
                ],

                columns: dtCols,
                rowId: (row) => safeCssId(row?.[opts.rowId]),
                createdRow: (tr, rowData) => {
                    const original = rowData?.[opts.rowId]
                    const safeRowId = safeCssId(original)

                    tr.dataset.rowid = original != null ? String(original) : ''
                    tr.dataset.rowsafeid = safeRowId
                    // 🔥 APPLY CUSTOM ROW CLASS IF PROVIDED
                    if (typeof opts.rowClass === 'function') {
                        const cls = opts.rowClass(rowData)
                        if (cls) {
                            tr.classList.add(cls)
                        }
                    }
                    // 🔥 ADD ID / CLASS FOR EACH TD
                    $('td', tr).each(function (index) {

                        // get column name from columns config
                        const col = dtCols[index]

                        // prefer "data" field, fallback to label
                        const columnName = col?.data || opts.columnsLabels[index]

                        if (!columnName) return

                        const safeColumn = safeCssId(columnName)

                        const cellId = `${safeRowId}_${safeColumn}`

                        // Add ID
                        this.id = cellId

                        // Add class
                        this.classList.add(`col-${safeColumn}`)
                        this.classList.add(`row-${safeRowId}`)
                    })
                },
                headerCallback: (thead) => { $(thead).find('tr').addClass('fw-semibold fs-6 text-gray-800 border-bottom border-gray-200') },
                drawCallback: () => { if (window.KTMenu?.createInstances) KTMenu.createInstances() },

                buttons: [
                    { extend: 'copyHtml5', name: 'copyButton', title: opts.exportTitle, exportOptions: { columns: `:not(${opts.exportExcludeSelector})` } },
                    // ✅ FIXED TYPO: exportExcludeSelectord -> exportExcludeSelector
                    { extend: 'csvHtml5', name: 'csvButton', title: opts.exportTitle, exportOptions: { columns: `:not(${opts.exportExcludeSelector})` } },
                    { extend: 'excelHtml5', name: 'excelButton', title: opts.exportTitle, exportOptions: { columns: `:not(${opts.exportExcludeSelector})` } },
                    {
                        extend: 'pdfHtml5',
                        title: opts.exportTitle || 'Export',
                        name: 'pdfButton',
                        pageSize: 'LEGAL',
                        orientation: 'landscape',
                        exportOptions: { columns: `:not(${opts.exportExcludeSelector})` },
                        customize: function (doc) {
                            doc.pageMargins = [10, 10, 10, 10];
                            doc.defaultStyle.fontSize = 8;
                            doc.defaultStyle.noWrap = false;

                            const tableNode = doc.content.find(x => x.table);
                            if (!tableNode || !tableNode.table || !tableNode.table.body || !tableNode.table.body.length) return;

                            const table = tableNode.table;
                            const body = table.body;
                            const colCount = body[0].length;

                            // safer page width calculation
                            const pageWidth = (doc.pageSize.width || 1000) - doc.pageMargins[0] - doc.pageMargins[2];

                            // helper: get plain text from pdfmake cell
                            const getCellText = (cell) => {
                                if (cell == null) return '';
                                if (typeof cell === 'string' || typeof cell === 'number' || typeof cell === 'boolean') {
                                    return String(cell);
                                }
                                if (typeof cell === 'object') {
                                    if (Array.isArray(cell.text)) {
                                        return cell.text.map(x => typeof x === 'object' ? (x.text || '') : x).join(' ');
                                    }
                                    return String(cell.text || '');
                                }
                                return '';
                            };

                            // style all cells
                            for (let r = 0; r < body.length; r++) {
                                for (let c = 0; c < colCount; c++) {
                                    if (body[r][c] == null || typeof body[r][c] !== 'object') {
                                        body[r][c] = { text: getCellText(body[r][c]) };
                                    }

                                    body[r][c].alignment = 'center';
                                    body[r][c].noWrap = false;
                                    body[r][c].margin = [2, 4, 2, 4];

                                    // keep header slightly bold
                                    if (r === 0) {
                                        body[r][c].bold = true;
                                    }
                                }
                            }

                            // detect content weight per column
                            const maxLens = Array(colCount).fill(0);

                            for (let c = 0; c < colCount; c++) {
                                for (let r = 0; r < body.length; r++) {
                                    const txt = getCellText(body[r][c]).replace(/\s+/g, ' ').trim();
                                    maxLens[c] = Math.max(maxLens[c], txt.length);
                                }
                            }

                            // convert text lengths to desired pixel-like widths
                            // tuned so small tables stay wider and large tables still wrap
                            let desiredWidths = maxLens.map((len, idx) => {
                                // base width from text length
                                let w = 24 + (len * 4.2);

                                // common narrow columns
                                const header = getCellText(body[0][idx]).toLowerCase();
                                if (header.includes('status') || header.includes('role') || header.includes('shift')) {
                                    w = Math.min(w, 60);
                                }
                                if (header.includes('date')) {
                                    w = Math.min(Math.max(w, 58), 72);
                                }
                                if (header.includes('is manager')) {
                                    w = Math.min(Math.max(w, 55), 65);
                                }
                                if (header.includes('email')) {
                                    w = Math.max(w, 95);
                                }
                                if (header.includes('manager') || header.includes('department')) {
                                    w = Math.max(w, 75);
                                }

                                // global min/max
                                return Math.max(45, Math.min(w, 110));
                            });

                            // reserve a little space for borders/padding
                            const usableWidth = pageWidth - (colCount * 6);

                            // if total desired is smaller than available width, stretch proportionally
                            // if bigger, shrink proportionally
                            const desiredTotal = desiredWidths.reduce((a, b) => a + b, 0);
                            const scale = usableWidth / desiredTotal;

                            let scaledWidths = desiredWidths.map(w => Math.round(w * scale));

                            // second clamp after scaling
                            scaledWidths = scaledWidths.map(w => Math.max(40, w));

                            // final correction so sum fits exactly
                            let finalTotal = scaledWidths.reduce((a, b) => a + b, 0);
                            let diff = usableWidth - finalTotal;

                            while (diff !== 0) {
                                for (let i = 0; i < scaledWidths.length && diff !== 0; i++) {
                                    if (diff > 0) {
                                        scaledWidths[i] += 1;
                                        diff -= 1;
                                    } else if (diff < 0 && scaledWidths[i] > 40) {
                                        scaledWidths[i] -= 1;
                                        diff += 1;
                                    }
                                }
                            }

                            table.widths = scaledWidths;

                            // slightly reduce font only for very wide tables
                            if (colCount >= 11) {
                                doc.defaultStyle.fontSize = 7;
                            }
                            if (colCount >= 13) {
                                doc.defaultStyle.fontSize = 6;
                            }

                            table.layout = {
                                hLineWidth: function () { return 0.5; },
                                vLineWidth: function () { return 0.5; },
                                hLineColor: function () { return '#aaa'; },
                                vLineColor: function () { return '#aaa'; },
                                paddingLeft: function () { return 2; },
                                paddingRight: function () { return 2; },
                                paddingTop: function () { return 3; },
                                paddingBottom: function () { return 3; }
                            };
                        }
                    },
                    { extend: 'print', name: 'printButton', title: opts.exportTitle, exportOptions: { columns: `:not(${opts.exportExcludeSelector})` } },
                ],
                initComplete: function () {
                    // If you sometimes don't have export buttons in DOM, guard these
                    const btnCopy = document.querySelector("#btnCopyToClipboard")
                    if (btnCopy) btnCopy.addEventListener("click", function () {
                        dt.buttons('copyButton:name').trigger();
                    });

                    const btnCsv = document.querySelector("#btnExportToCSV")
                    if (btnCsv) btnCsv.addEventListener("click", function () {
                        dt.buttons('csvButton:name').trigger();
                    });

                    const btnXls = document.querySelector("#btnExportToExcel")
                    if (btnXls) btnXls.addEventListener("click", function () {
                        dt.buttons('excelButton:name').trigger();
                    });

                    const btnPdf = document.querySelector("#btnExportToPDF")
                    if (btnPdf) btnPdf.addEventListener("click", function () {
                        dt.buttons('pdfButton:name').trigger();
                    });

                    const btnPrint = document.querySelector("#btnExportToPrint")
                    if (btnPrint) btnPrint.addEventListener("click", function () {
                        dt.buttons('printButton:name').trigger();
                    });
                },
            })

            this._tables.set(opts.id, dt)
            return dt
        },

        // Minimal public helpers
        get(id) { return this._tables.get(id) || $(`#${id}`).DataTable() },
        reload(id, resetPaging = false) { const dt = this.get(id); dt && dt.ajax.reload(null, !!resetPaging) },

        // Action menu builder (adds .action-trigger automatically)
        menu(row, items) {
            // ---- if no actions allowed, show disabled Actions button ----
            if (!items || !items.length) {
                return `
            <a href="javascript:void(0)"
               class="btn btn-sm btn-light btn-flex btn-center btn-disabled"
               data-kt-menu-trigger="click"
               data-kt-menu-placement="bottom-end"
               data-kt-menu-append-to="body">
               Actions <i class="ki-outline ki-down fs-5 ms-1"></i>
            </a>`;
            }

            // ---- normal menu rendering ----
            const menuItems = (items || []).map(it => {
                const label = typeof it.text === 'function' ? it.text(row) : it.text
                const extra = typeof it.attrs === 'function' ? it.attrs(row) : (it.attrs || {})
                const dataAttrs = Object.entries(extra).map(([k, v]) => ` data-${k}="${String(v)}"`).join('')
                return `<div class="menu-item px-3">
                  <a href="javascript:void(0)" class="menu-link px-3"
                     data-dg-action="${it.key}"${dataAttrs}>${label}</a>
                </div>`
            }).join('')

            return `
        <a href="javascript:void(0)"
           class="btn btn-sm btn-light btn-flex btn-center btn-active-light-primary action-trigger"
           data-kt-menu-trigger="click"
           data-kt-menu-placement="bottom-end"
           data-kt-menu-append-to="body">
           Actions <i class="ki-outline ki-down fs-5 ms-1"></i>
        </a>
        <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded
                    menu-gray-600 menu-state-bg-light-primary fw-semibold fs-7 w-175px py-4"
            data-kt-menu="true">
            ${menuItems}
        </div>`
        },

        // Delegate actions (one-liner; no extra event bus)
        delegate(container, handlers) {
            const $root = $(container); if (!$root.length) return
            $root.off('click.dgAction').on('click.dgAction', '[data-dg-action]', (e) => {
                e.preventDefault()
                const el = e.currentTarget
                const $tr = $(el).closest('tr')
                const id = $tr.data('rowid')
                const dt = $root.closest('table').DataTable()
                const action = $(el).data('dg-action')
                const fn = handlers?.[action]
                if (typeof fn === 'function') fn({ id, el, row: dt?.row($tr).data(), table: dt })
            })
        }
    }

    window.DataGrid = DataGrid
})()