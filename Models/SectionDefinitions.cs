namespace REM.Models;

public static class SectionDefinitions
{
    public const string Experience = "Experience";
    public const string Education = "Education";
    public const string Skills = "Skills";
    public const string Projects = "Projects";
    public const string Certifications = "Certifications";
    public const string Languages = "Languages";

    public static IReadOnlyList<string> AllSections { get; } =
    [
        Experience,
        Education,
        Skills,
        Projects,
        Certifications,
        Languages
    ];
}
