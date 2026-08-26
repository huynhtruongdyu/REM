namespace REM.Models;

public class Experience
{
    public Guid Id { get; set; }

    public string Company { get; set; } = "";

    public string Position { get; set; } = "";

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsCurrent { get; set; }

    public string Description { get; set; } = "";
}