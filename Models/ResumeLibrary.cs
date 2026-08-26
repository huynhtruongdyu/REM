namespace REM.Models;

public class ResumeEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Resume";
    public ResumeDocument Resume { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

public class ResumeLibrary
{
    public List<ResumeEntry> Resumes { get; set; } = [];
    public string? ActiveId { get; set; }

    public ResumeEntry? GetActive()
        => Resumes.FirstOrDefault(r => r.Id == ActiveId) ?? Resumes.FirstOrDefault();
}
