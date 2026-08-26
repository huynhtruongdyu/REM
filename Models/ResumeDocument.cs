namespace REM.Models;

public class ResumeDocument
{
    public PersonalInfo Personal { get; set; } = new();

    public List<Experience> Experiences { get; set; } = [];

    public List<Education> Educations { get; set; } = [];

    public List<Skill> Skills { get; set; } = [];

    public List<Project> Projects { get; set; } = [];

    public List<Certification> Certifications { get; set; } = [];

    public List<Language> Languages { get; set; } = [];
}