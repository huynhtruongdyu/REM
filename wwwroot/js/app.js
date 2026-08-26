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
