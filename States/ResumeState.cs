using Microsoft.AspNetCore.Components.Forms;
using REM.Models;

namespace REM.States;

public class ResumeState
{
    public ResumeDocument Resume { get; private set; } = new();

    public EditContext EditContext { get; private set; }

    public bool IsDirty { get; private set; }

    public event Action? OnChange;

    public ResumeState()
    {
        EditContext = CreateContext(Resume);
    }

    public void SetResume(ResumeDocument resume)
    {
        Resume = resume;
        EditContext = CreateContext(Resume);
        IsDirty = false;
        NotifyStateChanged();
    }

    public void MarkDirty()
    {
        IsDirty = true;
        NotifyStateChanged();
    }

    private EditContext CreateContext(ResumeDocument model)
    {
        var context = new EditContext(model);
        context.OnFieldChanged += (_, _) => MarkDirty();
        return context;
    }

    private void NotifyStateChanged()
        => OnChange?.Invoke();
}
