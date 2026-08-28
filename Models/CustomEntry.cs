namespace REM.Models;

public class CustomEntry
{
    public Guid Id { get; set; }

    public string Title { get; set; } = "";

    public string Subtitle { get; set; } = "";

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsCurrent { get; set; }

    public string Description { get; set; } = "";
}
