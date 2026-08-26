namespace REM.Models;

public class Project
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public string Url { get; set; } = "";

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public List<string> Technologies { get; set; } = [];
}
