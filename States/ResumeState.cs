using System.Collections.Generic;
using System.Linq;
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

    public ResumeLibrary Library { get; private set; } = new();

    public string? ActiveId { get; private set; }

    public IEnumerable<ResumeEntry> Resumes => Library.Resumes;

    public string ActiveName => Library.GetActive()?.Name ?? "";

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

    public void LoadLibrary(ResumeLibrary library)
    {
        Library = library;
        var active = Library.GetActive();
        if (active is null)
        {
            active = new ResumeEntry { Name = "My Resume", Resume = SampleResume.Create() };
            Library.Resumes.Add(active);
            Library.ActiveId = active.Id;
        }

        ActiveId = active.Id;
        Library.ActiveId = active.Id;
        SetResume(active.Resume, recordHistory: false);
        _undo.Clear();
        _redo.Clear();
    }

    public void SetActive(string id)
    {
        if (id == ActiveId)
        {
            return;
        }

        var entry = Library.Resumes.FirstOrDefault(r => r.Id == id);
        if (entry is null)
        {
            return;
        }

        ActiveId = id;
        Library.ActiveId = id;
        SetResume(entry.Resume, recordHistory: false);
        _undo.Clear();
        _redo.Clear();
    }

    public void CreateResume(string name, bool fromSample)
    {
        var doc = fromSample ? SampleResume.Create() : new ResumeDocument();
        var entry = new ResumeEntry { Name = name, Resume = doc };
        Library.Resumes.Add(entry);
        ActiveId = entry.Id;
        Library.ActiveId = entry.Id;
        SetResume(doc, recordHistory: false);
        _undo.Clear();
        _redo.Clear();
    }

    public void DuplicateResume(string id)
    {
        var source = Library.Resumes.FirstOrDefault(r => r.Id == id);
        if (source is null)
        {
            return;
        }

        var clone = JsonSerializer.Deserialize<ResumeDocument>(JsonSerializer.Serialize(source.Resume, JsonOptions))!;
        var entry = new ResumeEntry { Name = source.Name + " copy", Resume = clone };
        Library.Resumes.Add(entry);
        ActiveId = entry.Id;
        Library.ActiveId = entry.Id;
        SetResume(clone, recordHistory: false);
        _undo.Clear();
        _redo.Clear();
    }

    public void RenameResume(string id, string name)
    {
        var entry = Library.Resumes.FirstOrDefault(r => r.Id == id);
        if (entry is null)
        {
            return;
        }

        entry.Name = string.IsNullOrWhiteSpace(name) ? "Untitled" : name.Trim();
        NotifyStateChanged();
    }

    public void DeleteResume(string id)
    {
        var entry = Library.Resumes.FirstOrDefault(r => r.Id == id);
        if (entry is null)
        {
            return;
        }

        Library.Resumes.Remove(entry);
        if (Library.Resumes.Count == 0)
        {
            CreateResume("My Resume", fromSample: true);
            return;
        }

        if (ActiveId == id)
        {
            SetActive(Library.Resumes[0].Id);
        }
        else
        {
            NotifyStateChanged();
        }
    }

    public void ImportLibrary(ResumeLibrary imported)
    {
        var added = new List<string>();
        foreach (var entry in imported.Resumes)
        {
            var clone = JsonSerializer.Deserialize<ResumeDocument>(JsonSerializer.Serialize(entry.Resume, JsonOptions))!;
            var newEntry = new ResumeEntry { Name = entry.Name, Resume = clone, UpdatedAt = DateTime.Now };
            Library.Resumes.Add(newEntry);
            added.Add(newEntry.Id);
        }

        if (added.Count > 0)
        {
            ActiveId = added[0];
            Library.ActiveId = added[0];
            SetResume(Library.GetActive()!.Resume, recordHistory: false);
            _undo.Clear();
            _redo.Clear();
        }
        else
        {
            NotifyStateChanged();
        }
    }

    public void SetResume(ResumeDocument resume, bool recordHistory = true)
    {
        if (recordHistory)
        {
            _undo.Push(_committed);
            _redo.Clear();
        }

        Resume = resume;
        var active = Library.GetActive();
        if (active is not null)
        {
            active.Resume = resume;
        }

        EditContext = CreateContext(resume);
        _committed = Serialize(resume);
        IsDirty = false;
        NotifyStateChanged();
        OnHistoryChange?.Invoke();
    }

    public void MarkDirty()
    {
        IsDirty = true;
        var active = Library.GetActive();
        if (active is not null)
        {
            active.UpdatedAt = DateTime.Now;
        }

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
        var active = Library.GetActive();
        if (active is not null)
        {
            active.Resume = doc;
        }

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
