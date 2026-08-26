using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Components.Forms;
using REM.Models;

namespace REM.States;

public class ResumeState
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    private readonly Stack<string> _undo = new();
    private readonly Stack<string> _redo = new();
    private readonly SynchronizationContext? _syncContext;
    private readonly System.Timers.Timer _commitTimer;
    private string _committed;

    public ResumeDocument Resume { get; private set; } = new();

    public EditContext EditContext { get; private set; }

    public bool IsDirty { get; private set; }

    public bool CanUndo => _undo.Count > 0;

    public bool CanRedo => _redo.Count > 0;

    public event Action? OnChange;

    public event Action? OnHistoryChange;

    public ResumeState()
    {
        _syncContext = SynchronizationContext.Current;
        _committed = Serialize(Resume);
        EditContext = CreateContext(Resume);

        _commitTimer = new System.Timers.Timer(500) { AutoReset = false };
        _commitTimer.Elapsed += (_, _) =>
        {
            var current = Serialize(Resume);
            if (current != _committed)
            {
                PushToUi(() =>
                {
                    _undo.Push(_committed);
                    _committed = current;
                    _redo.Clear();
                    OnHistoryChange?.Invoke();
                });
            }
        };
    }

    public void SetResume(ResumeDocument resume, bool recordHistory = true)
    {
        if (recordHistory)
        {
            _undo.Push(_committed);
            _redo.Clear();
        }

        Resume = resume;
        EditContext = CreateContext(resume);
        _committed = Serialize(resume);
        IsDirty = false;
        NotifyStateChanged();
        OnHistoryChange?.Invoke();
    }

    public void MarkDirty()
    {
        IsDirty = true;
        NotifyStateChanged();
        _commitTimer.Stop();
        _commitTimer.Start();
    }

    public void MarkSaved()
    {
        IsDirty = false;
        NotifyStateChanged();
    }

    public void Undo()
    {
        if (_undo.Count == 0)
        {
            return;
        }

        _redo.Push(_committed);
        _committed = _undo.Pop();
        Apply(_committed, isDirty: true);
        OnHistoryChange?.Invoke();
    }

    public void Redo()
    {
        if (_redo.Count == 0)
        {
            return;
        }

        _undo.Push(_committed);
        _committed = _redo.Pop();
        Apply(_committed, isDirty: true);
        OnHistoryChange?.Invoke();
    }

    private void Apply(string serialized, bool isDirty)
    {
        var doc = Deserialize(serialized) ?? new ResumeDocument();
        Resume = doc;
        EditContext = CreateContext(doc);
        _committed = serialized;
        IsDirty = isDirty;
        NotifyStateChanged();
    }

    private EditContext CreateContext(ResumeDocument model)
    {
        var context = new EditContext(model);
        context.OnFieldChanged += (_, _) => MarkDirty();
        return context;
    }

    private string Serialize(ResumeDocument doc)
        => JsonSerializer.Serialize(doc, JsonOptions);

    private static ResumeDocument? Deserialize(string json)
        => JsonSerializer.Deserialize<ResumeDocument>(json);

    private void PushToUi(Action action)
    {
        if (_syncContext is null)
        {
            action();
        }
        else
        {
            _syncContext.Post(_ => action(), null);
        }
    }

    private void NotifyStateChanged()
        => OnChange?.Invoke();
}
