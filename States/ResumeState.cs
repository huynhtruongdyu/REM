using REM.Models;

namespace REM.States;

public class ResumeState
{
    public ResumeDocument Resume { get; private set; } = new();

    public bool IsDirty { get; private set; }

    public event Action? OnChange;

    public void SetResume(ResumeDocument resume)
    {
        Resume = resume;
        IsDirty = false;
        NotifyStateChanged();
    }

    public void MarkDirty()
    {
        IsDirty = true;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
        => OnChange?.Invoke();
}