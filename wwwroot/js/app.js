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

window.getPdfOptions = function () {
    return {
        margin: [0.3, 0.3, 0.3, 0.3],
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2, useCORS: true, backgroundColor: '#ffffff' },
        jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' },
        pagebreak: { mode: ['css', 'legacy'] }
    };
};

window.exportPdf = function (filename) {
    var el = document.querySelector('.resume-preview');
    if (!el || typeof html2pdf === 'undefined') {
        window.print();
        return;
    }

    var opt = window.getPdfOptions();
    opt.filename = filename || 'resume.pdf';

    html2pdf().set(opt).from(el).save();
};

window.previewPdf = function () {
    var el = document.querySelector('.resume-preview');
    if (!el || typeof html2pdf === 'undefined') {
        window.print();
        return;
    }

    html2pdf().set(window.getPdfOptions()).from(el).outputPdf('blob').then(function (blob) {
        var url = URL.createObjectURL(blob);
        var tab = window.open(url, '_blank');
        if (!tab) {
            location.href = url;
        }
        setTimeout(function () { URL.revokeObjectURL(url); }, 60000);
    });
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
