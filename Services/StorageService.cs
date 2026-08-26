using System.Text;
using System.Text.Json;
using REM.Models;
using Microsoft.JSInterop;

namespace REM.Services;

public class StorageService
{
    private const string LibraryKey = "rem-library";

    private readonly IJSRuntime _js;

    public StorageService(IJSRuntime js)
    {
        _js = js;
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public async Task SaveLibraryAsync(ResumeLibrary library)
        => await _js.InvokeVoidAsync("storage.save", LibraryKey, JsonSerializer.Serialize(library, Options));

    public async Task<ResumeLibrary> LoadLibraryAsync()
    {
        var json = await _js.InvokeAsync<string?>("storage.load", LibraryKey);
        if (!string.IsNullOrWhiteSpace(json))
        {
            var lib = JsonSerializer.Deserialize<ResumeLibrary>(json);
            if (lib is not null)
            {
                return lib;
            }
        }

        // Seed a fresh library with the sample resume.
        var seeded = new ResumeLibrary();
        var seed = new ResumeEntry { Name = "My Resume", Resume = SampleResume.Create() };
        seeded.Resumes.Add(seed);
        seeded.ActiveId = seed.Id;
        return seeded;
    }

    public async Task ExportAsync(ResumeDocument doc)
        => await _js.InvokeVoidAsync("storage.downloadFile", "resume.json", JsonSerializer.Serialize(doc, Options));

    public async Task ExportLibraryAsync(ResumeLibrary library)
        => await _js.InvokeVoidAsync("storage.downloadFile", "rem-library.json", JsonSerializer.Serialize(library, Options));

    public ResumeDocument ImportResume(string json)
        => JsonSerializer.Deserialize<ResumeDocument>(json)
        ?? throw new InvalidOperationException("Invalid resume file.");

    public ResumeLibrary? TryImportLibrary(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("Resumes", out var res) && res.ValueKind == JsonValueKind.Array)
            {
                var lib = JsonSerializer.Deserialize<ResumeLibrary>(json);
                if (lib is not null && lib.Resumes.Count > 0)
                {
                    return lib;
                }
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    public static string ToMarkdown(ResumeDocument doc)
    {
        var sb = new StringBuilder();
        var p = doc.Personal;

        if (!string.IsNullOrWhiteSpace(p.FullName))
        {
            sb.AppendLine($"# {p.FullName}");
        }

        if (!string.IsNullOrWhiteSpace(p.Title))
        {
            sb.AppendLine($"**{p.Title}**");
        }

        var contact = new[]
        {
            p.Email, p.Phone, p.Location, p.Website, p.LinkedIn, p.GitHub
        }.Where(v => !string.IsNullOrWhiteSpace(v));

        if (contact.Any())
        {
            sb.AppendLine(string.Join(" | ", contact));
        }

        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(p.Summary))
        {
            sb.AppendLine("## Summary");
            sb.AppendLine(p.Summary);
            sb.AppendLine();
        }

        foreach (var section in doc.SectionOrder)
        {
            switch (section)
            {
                case SectionDefinitions.Experience:
                    if (doc.Experiences.Count > 0)
                    {
                        sb.AppendLine("## Experience");
                        foreach (var x in doc.Experiences)
                        {
                            sb.AppendLine($"### {x.Position} — {x.Company}");
                            sb.AppendLine(Range(x.StartDate, x.EndDate, x.IsCurrent));
                            if (!string.IsNullOrWhiteSpace(x.Description))
                            {
                                sb.AppendLine(x.Description);
                            }

                            sb.AppendLine();
                        }
                    }

                    break;
                case SectionDefinitions.Education:
                    if (doc.Educations.Count > 0)
                    {
                        sb.AppendLine("## Education");
                        foreach (var x in doc.Educations)
                        {
                            var field = string.IsNullOrWhiteSpace(x.FieldOfStudy) ? "" : $", {x.FieldOfStudy}";
                            sb.AppendLine($"### {x.Degree}{field} — {x.Institution}");
                            sb.AppendLine(Range(x.StartDate, x.EndDate, false));
                            if (!string.IsNullOrWhiteSpace(x.Gpa))
                            {
                                sb.AppendLine($"GPA: {x.Gpa}");
                            }

                            if (!string.IsNullOrWhiteSpace(x.Description))
                            {
                                sb.AppendLine(x.Description);
                            }

                            sb.AppendLine();
                        }
                    }

                    break;
                case SectionDefinitions.Skills:
                    if (doc.Skills.Count > 0)
                    {
                        sb.AppendLine("## Skills");
                        sb.AppendLine(string.Join(", ", doc.Skills
                            .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                            .Select(s => string.IsNullOrWhiteSpace(s.Proficiency) ? s.Name : $"{s.Name} ({s.Proficiency})")));
                        sb.AppendLine();
                    }

                    break;
                case SectionDefinitions.Projects:
                    if (doc.Projects.Count > 0)
                    {
                        sb.AppendLine("## Projects");
                        foreach (var x in doc.Projects)
                        {
                            sb.AppendLine($"### {x.Name}");
                            if (!string.IsNullOrWhiteSpace(x.Url))
                            {
                                sb.AppendLine(x.Url);
                            }

                            if (!string.IsNullOrWhiteSpace(x.Description))
                            {
                                sb.AppendLine(x.Description);
                            }

                            if (x.Technologies.Count > 0)
                            {
                                sb.AppendLine("Technologies: " + string.Join(", ", x.Technologies));
                            }

                            sb.AppendLine();
                        }
                    }

                    break;
                case SectionDefinitions.Certifications:
                    if (doc.Certifications.Count > 0)
                    {
                        sb.AppendLine("## Certifications");
                        foreach (var x in doc.Certifications)
                        {
                            sb.AppendLine($"### {x.Name} — {x.Issuer}");
                            var years = $"{x.IssueDate.Year}{(x.ExpiryDate.HasValue ? $" – {x.ExpiryDate.Value.Year}" : "")}";
                            sb.AppendLine(years);
                            if (!string.IsNullOrWhiteSpace(x.CredentialId))
                            {
                                sb.AppendLine($"Credential ID: {x.CredentialId}");
                            }

                            sb.AppendLine();
                        }
                    }

                    break;
                case SectionDefinitions.Languages:
                    if (doc.Languages.Count > 0)
                    {
                        sb.AppendLine("## Languages");
                        sb.AppendLine(string.Join(", ", doc.Languages
                            .Where(l => !string.IsNullOrWhiteSpace(l.Name))
                            .Select(l => string.IsNullOrWhiteSpace(l.Proficiency) ? l.Name : $"{l.Name} ({l.Proficiency})")));
                        sb.AppendLine();
                    }

                    break;
            }
        }

        return sb.ToString().TrimEnd() + "\n";
    }

    private static string Range(DateOnly? start, DateOnly? end, bool current)
    {
        if (!start.HasValue && !end.HasValue)
        {
            return "";
        }

        var endText = current ? "Present" : (end.HasValue ? end.Value.ToString("MMM yyyy") : "");
        var startText = start.HasValue ? start.Value.ToString("MMM yyyy") : "";
        return $"{startText} – {endText}";
    }
}
