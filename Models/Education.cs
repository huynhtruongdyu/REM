namespace REM.Models;

public class Education
{
    public Guid Id { get; set; }

    public string Institution { get; set; } = "";

    public string Degree { get; set; } = "";

    public string FieldOfStudy { get; set; } = "";

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string Description { get; set; } = "";

    public string Gpa { get; set; } = "";
}
