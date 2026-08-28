window.sortableInterop = {
    init: function (element, dotNetRef, group, handle) {
        if (!element || typeof Sortable === 'undefined') return;
        if (element._sortable) element._sortable.destroy();
        element._sortable = Sortable.create(element, {
            animation: 150,
            handle: handle || '.drag-handle',
            draggable: '.sortable-item',
            group: group || undefined,
            onEnd: function (evt) {
                if (evt.oldIndex !== evt.newIndex) {
                    dotNetRef.invokeMethodAsync('OnDrop', evt.oldIndex, evt.newIndex);
                }
            }
        });
    },
    destroy: function (element) {
        if (element && element._sortable) {
            element._sortable.destroy();
            element._sortable = null;
        }
    }
};

window.storage = {
    save: function (key, value) {
        localStorage.setItem(key, value);
    },
    load: function (key) {
        return localStorage.getItem(key);
    },
    downloadFile: function (filename, content) {
        var blob = new Blob([content], { type: 'application/json' });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }
};

window.clickEl = function (id) {
    var el = document.getElementById(id);
    if (el) el.click();
};

window.setDocumentTitle = function (title) {
    document.title = title || 'Resume Builder — REM';
};

window.previewPdf = function () {
    printWithoutTitle();
};

window.exportTextPdf = function (filename, json) {
    if (typeof pdfMake === 'undefined') {
        printWithoutTitle();
        return;
    }

    pdfMake.createPdf(JSON.parse(json)).download(filename || 'resume.pdf');
};

function printWithoutTitle() {
    var original = document.title;
    document.title = '';
    var restore = function () {
        document.title = original;
        window.removeEventListener('afterprint', restore);
    };
    window.addEventListener('afterprint', restore);
    window.print();
}

window.exportImagePdf = function (filename) {
    var el = document.querySelector('.resume-preview');
    if (!el || typeof html2pdf === 'undefined') {
        window.print();
        return;
    }

    var opt = {
        margin: [0.3, 0.3, 0.3, 0.3],
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2, useCORS: true, backgroundColor: '#ffffff' },
        jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' },
        pagebreak: { mode: ['css', 'legacy'], avoid: ['.rp-header', '.rp-section', '.rp-entry'] }
    };
    opt.filename = filename || 'resume.pdf';

    html2pdf().set(opt).from(el).save();
};

window.theme = {
    KEY: 'rem-color-mode',
    apply: function (mode) {
        if (mode !== 'dark' && mode !== 'light') return 'light';
        document.documentElement.setAttribute('data-color-mode', mode);
        try { localStorage.setItem(window.theme.KEY, mode); } catch (e) { }
        return mode;
    },
    toggle: function () {
        var current = document.documentElement.getAttribute('data-color-mode');
        return window.theme.apply(current === 'dark' ? 'light' : 'dark');
    },
    get: function () {
        return document.documentElement.getAttribute('data-color-mode') || 'light';
    },
    init: function () {
        var saved;
        try { saved = localStorage.getItem(window.theme.KEY); } catch (e) { saved = null; }
        if (saved === 'dark' || saved === 'light') {
            window.theme.apply(saved);
        } else if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            window.theme.apply('dark');
        }
    }
};

window.shortcuts = {
    register: function (dotNetRef) {
        window.__resumeShortcuts = function (e) {
            if (!(e.ctrlKey || e.metaKey)) return;
            var key = e.key.toLowerCase();
            if (key === 'z' && !e.shiftKey) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('Undo');
            } else if ((key === 'z' && e.shiftKey) || key === 'y') {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('Redo');
            }
        };
        window.addEventListener('keydown', window.__resumeShortcuts);
    },
    unregister: function () {
        if (window.__resumeShortcuts) {
            window.removeEventListener('keydown', window.__resumeShortcuts);
            window.__resumeShortcuts = null;
        }
    }
};
