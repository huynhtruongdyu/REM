using System.Linq;
using System.Text.Json.Nodes;
using REM.Models;

namespace REM.Services;

public static class PdfService
{
    public static string BuildDocDefinitionJson(ResumeDocument doc)
    {
        var (accent, _, accentInk) = SchemeAccent(doc.ColorScheme);
        var content = new JsonArray();
        var p = doc.Personal;

        if (!string.IsNullOrWhiteSpace(p.FullName))
        {
            content.Add(Text(p.FullName, "name"));
        }

        if (!string.IsNullOrWhiteSpace(p.Title))
        {
            content.Add(Text(p.Title, "title"));
        }

        var contactNodes = ContactNodes(p);
        if (contactNodes.Count > 0)
        {
            content.Add(new JsonObject { ["text"] = contactNodes });
        }

        if (!string.IsNullOrWhiteSpace(p.Summary))
        {
            AddSectionHeader(content, "Summary", accent);
            content.Add(Text(p.Summary, "desc"));
        }

        foreach (var section in doc.SectionOrder)
        {
            if (doc.HiddenSections.Contains(section))
            {
                continue;
            }

            switch (section)
            {
                case SectionDefinitions.Experience:
                    if (doc.Experiences.Count > 0)
                    {
                        AddSectionHeader(content, "Experience", accent);
                        foreach (var x in doc.Experiences)
                        {
                            AddEntryHead(content, $"{x.Position} — {x.Company}", Range(x.StartDate, x.EndDate, x.IsCurrent));
                            if (!string.IsNullOrWhiteSpace(x.Description))
                            {
                                content.Add(Text(x.Description, "desc"));
                            }
                        }
                    }

                    break;
                case SectionDefinitions.Education:
                    if (doc.Educations.Count > 0)
                    {
                        AddSectionHeader(content, "Education", accent);
                        foreach (var x in doc.Educations)
                        {
                            var field = string.IsNullOrWhiteSpace(x.FieldOfStudy) ? "" : $", {x.FieldOfStudy}";
                            AddEntryHead(content, $"{x.Degree}{field} — {x.Institution}", Range(x.StartDate, x.EndDate, false));
                            if (!string.IsNullOrWhiteSpace(x.Gpa))
                            {
                                content.Add(Text($"GPA: {x.Gpa}", "muted"));
                            }

                            if (!string.IsNullOrWhiteSpace(x.Description))
                            {
                                content.Add(Text(x.Description, "desc"));
                            }
                        }
                    }

                    break;
                case SectionDefinitions.Skills:
                    if (doc.Skills.Count > 0)
                    {
                        AddSectionHeader(content, "Skills", accent);
                        content.Add(Text(string.Join(", ", doc.Skills
                            .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                            .Select(s => string.IsNullOrWhiteSpace(s.Proficiency) ? s.Name : $"{s.Name} ({s.Proficiency})")), "desc"));
                    }

                    break;
                case SectionDefinitions.Projects:
                    if (doc.Projects.Count > 0)
                    {
                        AddSectionHeader(content, "Projects", accent);
                        foreach (var x in doc.Projects)
                        {
                            AddEntryHead(content, x.Name, "");
                            if (!string.IsNullOrWhiteSpace(x.Url))
                            {
                                var url = Href(x.Url);
                                if (url is not null)
                                {
                                    content.Add(new JsonObject { ["text"] = x.Url, ["link"] = url, ["style"] = "muted" });
                                }
                                else
                                {
                                    content.Add(Text(x.Url, "muted"));
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(x.Description))
                            {
                                content.Add(Text(x.Description, "desc"));
                            }

                            if (x.Technologies.Count > 0)
                            {
                                content.Add(Text("Technologies: " + string.Join(", ", x.Technologies), "muted"));
                            }
                        }
                    }

                    break;
                case SectionDefinitions.Certifications:
                    if (doc.Certifications.Count > 0)
                    {
                        AddSectionHeader(content, "Certifications", accent);
                        foreach (var x in doc.Certifications)
                        {
                            var years = $"{x.IssueDate.Year}{(x.ExpiryDate.HasValue ? $" – {x.ExpiryDate.Value.Year}" : "")}";
                            AddEntryHead(content, $"{x.Name} — {x.Issuer}", years);
                            if (!string.IsNullOrWhiteSpace(x.CredentialId))
                            {
                                content.Add(Text($"Credential ID: {x.CredentialId}", "muted"));
                            }
                        }
                    }

                    break;
                case SectionDefinitions.Languages:
                    if (doc.Languages.Count > 0)
                    {
                        AddSectionHeader(content, "Languages", accent);
                        content.Add(Text(string.Join(", ", doc.Languages
                            .Where(l => !string.IsNullOrWhiteSpace(l.Name))
                            .Select(l => string.IsNullOrWhiteSpace(l.Proficiency) ? l.Name : $"{l.Name} ({l.Proficiency})")), "desc"));
                    }

                    break;
            }
        }

        var docDef = new JsonObject
        {
            ["pageSize"] = "LETTER",
            ["pageMargins"] = new JsonArray(40, 40, 40, 40),
            ["defaultStyle"] = new JsonObject { ["fontSize"] = 10, ["color"] = "#1a1a1a" },
            ["styles"] = Styles(accent, accentInk),
            ["content"] = content
        };

        return docDef.ToJsonString();
    }

    private static JsonObject Text(string text, string style)
        => new() { ["text"] = text, ["style"] = style };

    private static void AddSectionHeader(JsonArray content, string label, string accent)
    {
        content.Add(Text(label.ToUpperInvariant(), "section"));
        content.Add(new JsonObject
        {
            ["canvas"] = new JsonArray(new JsonObject
            {
                ["type"] = "line",
                ["x1"] = 0,
                ["y1"] = 0,
                ["x2"] = 532,
                ["y2"] = 0,
                ["lineWidth"] = 1,
                ["lineColor"] = accent
            }),
            ["margin"] = new JsonArray(0, 2, 0, 6)
        });
    }

    private static void AddEntryHead(JsonArray content, string left, string right)
    {
        var columns = new JsonArray
        {
            new JsonObject { ["text"] = left, ["style"] = "entryTitle", ["width"] = "*" }
        };

        if (!string.IsNullOrWhiteSpace(right))
        {
            columns.Add(new JsonObject { ["text"] = right, ["style"] = "dates", ["width"] = "auto" });
        }

        content.Add(new JsonObject
        {
            ["columns"] = columns,
            ["margin"] = new JsonArray(0, 4, 0, 0)
        });
    }

    private static JsonObject Styles(string accent, string accentInk)
    {
        return new JsonObject
        {
            ["name"] = new JsonObject { ["fontSize"] = 20, ["bold"] = true, ["color"] = accentInk },
            ["title"] = new JsonObject { ["fontSize"] = 12, ["color"] = accent, ["margin"] = new JsonArray(0, 0, 0, 4) },
            ["contact"] = new JsonObject { ["fontSize"] = 9, ["color"] = "#444", ["margin"] = new JsonArray(0, 0, 0, 6) },
            ["section"] = new JsonObject { ["fontSize"] = 11, ["bold"] = true, ["color"] = accent, ["characterSpacing"] = 1, ["margin"] = new JsonArray(0, 10, 0, 2) },
            ["entryTitle"] = new JsonObject { ["bold"] = true, ["fontSize"] = 10.5 },
            ["dates"] = new JsonObject { ["fontSize"] = 9, ["color"] = "#666", ["alignment"] = "right" },
            ["desc"] = new JsonObject { ["fontSize"] = 9.5, ["color"] = "#333", ["margin"] = new JsonArray(0, 2, 0, 0) },
            ["muted"] = new JsonObject { ["fontSize"] = 9, ["color"] = "#666", ["margin"] = new JsonArray(0, 1, 0, 0) }
        };
    }

    private static (string accent, string soft, string ink) SchemeAccent(string scheme) => scheme switch
    {
        "indigo" => ("#4f46e5", "#e0e7ff", "#1e1b4b"),
        "emerald" => ("#059669", "#d1fae5", "#064e3b"),
        "rose" => ("#e11d48", "#ffe4e6", "#4c0519"),
        "amber" => ("#d97706", "#fef3c7", "#451a03"),
        "violet" => ("#7c3aed", "#ede9fe", "#2e1065"),
        "teal" => ("#0d9488", "#ccfbf1", "#042f2e"),
        "slate" => ("#475569", "#e2e8f0", "#0f172a"),
        "black" => ("#111111", "#e5e7eb", "#000000"),
        "red" => ("#dc2626", "#fee2e2", "#7f1d1d"),
        "orange" => ("#ea580c", "#ffedd5", "#7c2d12"),
        "pink" => ("#db2777", "#fce7f3", "#831843"),
        "cyan" => ("#0891b2", "#cffafe", "#164e63"),
        "lime" => ("#65a30d", "#ecfccb", "#365314"),
        "purple" => ("#9333ea", "#f3e8ff", "#4a1d96"),
        _ => ("#1b6ec2", "#cfe1f5", "#10243f")
    };

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

    private static JsonArray ContactNodes(PersonalInfo p)
    {
        var nodes = new JsonArray();
        void AddPart(string text, string? href)
        {
            if (nodes.Count > 0)
            {
                nodes.Add(new JsonObject { ["text"] = "   |   ", ["style"] = "contact" });
            }

            if (href is not null)
            {
                nodes.Add(new JsonObject { ["text"] = text, ["link"] = href, ["style"] = "contact" });
            }
            else
            {
                nodes.Add(Text(text, "contact"));
            }
        }

        if (!string.IsNullOrWhiteSpace(p.Email)) AddPart(p.Email!, EmailHref(p.Email));
        if (!string.IsNullOrWhiteSpace(p.Phone)) AddPart(p.Phone!, PhoneHref(p.Phone));
        if (!string.IsNullOrWhiteSpace(p.Location)) AddPart(p.Location!, null);
        if (!string.IsNullOrWhiteSpace(p.Website)) AddPart(p.Website!, Href(p.Website));
        if (!string.IsNullOrWhiteSpace(p.LinkedIn)) AddPart(p.LinkedIn!, Href(p.LinkedIn));
        if (!string.IsNullOrWhiteSpace(p.GitHub)) AddPart(p.GitHub!, Href(p.GitHub));
        return nodes;
    }

    private static string? Href(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var v = value.Trim();
        if (v.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (v.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || v.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return v;
        }

        if (v.StartsWith("//"))
        {
            return "https:" + v;
        }

        if (v.Contains('.') || v.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
        {
            return "https://" + v.TrimStart('/');
        }

        return null;
    }

    private static string? EmailHref(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        if (email!.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            return email;
        }

        return email.Contains('@') ? "mailto:" + email : Href(email);
    }

    private static string? PhoneHref(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var digits = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
        return digits.Length > 0 ? "tel:" + digits : null;
    }
}
