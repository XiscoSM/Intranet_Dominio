// Cuadre de cajas: expandable rows, "only issues" filter, column sorting, auto-submit filters.
(function () {
    'use strict';

    var form = document.getElementById('cq-form');
    var table = document.getElementById('cq-table');

    // Changing date or store reloads immediately (no "Consultar" button needed).
    if (form) {
        form.querySelectorAll('[data-autosubmit]').forEach(function (el) {
            el.addEventListener('change', function () { form.submit(); });
        });
    }
    if (!table) return;

    // ---- Expand / collapse cashier detail ----
    table.addEventListener('click', function (e) {
        var btn = e.target.closest('.cq-toggle');
        if (!btn) return;
        var detail = document.getElementById(btn.getAttribute('aria-controls'));
        var open = btn.getAttribute('aria-expanded') !== 'true';
        btn.setAttribute('aria-expanded', open ? 'true' : 'false');
        detail.hidden = !open;
    });

    // Clicking anywhere on the summary row (except links/buttons) also toggles it.
    table.addEventListener('click', function (e) {
        if (e.target.closest('a, button, input')) return;
        var row = e.target.closest('.cq-row');
        if (row) row.querySelector('.cq-toggle').click();
    });

    // ---- "Only cashiers to review" filter, kept in the URL (?solo=1) ----
    var only = document.getElementById('cq-only');
    var none = document.getElementById('cq-none');

    function applyFilter(on) {
        table.classList.toggle('only-issues', on);
        var visible = table.querySelectorAll('tbody.cq-cashier.issue').length;
        if (none) none.hidden = !(on && visible === 0);
        var url = new URL(window.location.href);
        if (on) url.searchParams.set('solo', '1'); else url.searchParams.delete('solo');
        history.replaceState(null, '', url);
    }

    if (only) {
        only.addEventListener('change', function () { applyFilter(only.checked); });
        applyFilter(only.checked);
    }
    document.querySelectorAll('[data-only-issues]').forEach(function (a) {
        a.addEventListener('click', function (e) {
            e.preventDefault();
            if (only) { only.checked = true; applyFilter(true); }
            table.scrollIntoView({ behavior: 'smooth', block: 'start' });
        });
    });

    // ---- Column sorting (each cashier is its own <tbody>, so detail rows move with it) ----
    table.querySelectorAll('.cq-sort').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var key = btn.getAttribute('data-sort');
            var current = btn.getAttribute('aria-sort');
            var dir = current === 'descending' ? 'ascending' : 'descending';
            if (!current && key === 'name') dir = 'ascending';

            table.querySelectorAll('.cq-sort').forEach(function (b) { b.removeAttribute('aria-sort'); });
            btn.setAttribute('aria-sort', dir);

            var bodies = Array.prototype.slice.call(table.querySelectorAll('tbody.cq-cashier'));
            bodies.sort(function (a, b) {
                var va = a.dataset[key], vb = b.dataset[key];
                var cmp = key === 'name'
                    ? va.localeCompare(vb, 'es')
                    : parseFloat(va) - parseFloat(vb);
                return dir === 'ascending' ? cmp : -cmp;
            });
            var foot = table.querySelector('tfoot');
            bodies.forEach(function (b) { table.insertBefore(b, foot); });
        });
    });
})();
