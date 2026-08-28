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

    public const string CustomPrefix = "custom:";

    public static string CustomKey(string id) => CustomPrefix + id;

    public static bool IsCustom(string key) => key.StartsWith(CustomPrefix, StringComparison.Ordinal);

    public static string CustomId(string key) => key[CustomPrefix.Length..];
}
