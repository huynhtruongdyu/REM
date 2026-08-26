namespace REM.Models;

public static class SampleResume
{
    public static ResumeDocument Create()
    {
        return new ResumeDocument
        {
            Personal = new PersonalInfo
            {
                FullName = "Jane Doe",
                Title = "Senior Software Engineer",
                Email = "jane.doe@example.com",
                Phone = "+1 (555) 123-4567",
                Location = "Seattle, WA",
                Website = "janedoe.dev",
                LinkedIn = "linkedin.com/in/janedoe",
                GitHub = "github.com/janedoe",
                Summary = "Results-driven software engineer with 8+ years building scalable web " +
                          "applications and leading cross-functional teams. Passionate about clean " +
                          "code, developer experience, and shipping customer value."
            },
            Experiences =
            [
                new Experience
                {
                    Id = Guid.NewGuid(),
                    Company = "Acme Corporation",
                    Position = "Senior Software Engineer",
                    StartDate = new DateOnly(2021, 3, 1),
                    EndDate = null,
                    IsCurrent = true,
                    Description = "Led migration of a monolith to microservices, cutting p99 latency by 40%. " +
                                  "Mentored a team of 5 engineers and established the frontend platform guild."
                },
                new Experience
                {
                    Id = Guid.NewGuid(),
                    Company = "Globex Inc.",
                    Position = "Software Engineer",
                    StartDate = new DateOnly(2017, 6, 1),
                    EndDate = new DateOnly(2021, 2, 28),
                    IsCurrent = false,
                    Description = "Built the customer portal used by 200k+ users. Introduced automated " +
                                  "testing that reduced regressions by 60%."
                }
            ],
            Educations =
            [
                new Education
                {
                    Id = Guid.NewGuid(),
                    Institution = "University of Washington",
                    Degree = "B.S.",
                    FieldOfStudy = "Computer Science",
                    StartDate = new DateOnly(2013, 9, 1),
                    EndDate = new DateOnly(2017, 5, 31),
                    Gpa = "3.8",
                    Description = "Graduated with honors. President of the Computing Club."
                }
            ],
            Skills =
            [
                new Skill { Id = Guid.NewGuid(), Name = "C# / .NET", Category = "Languages", Proficiency = "Expert" },
                new Skill { Id = Guid.NewGuid(), Name = "TypeScript", Category = "Languages", Proficiency = "Advanced" },
                new Skill { Id = Guid.NewGuid(), Name = "React", Category = "Frontend", Proficiency = "Advanced" },
                new Skill { Id = Guid.NewGuid(), Name = "Azure", Category = "Cloud", Proficiency = "Intermediate" },
                new Skill { Id = Guid.NewGuid(), Name = "SQL", Category = "Data", Proficiency = "Advanced" }
            ],
            Projects =
            [
                new Project
                {
                    Id = Guid.NewGuid(),
                    Name = "OpenMetrics Dashboard",
                    Url = "github.com/janedoe/openmetrics",
                    StartDate = new DateOnly(2022, 1, 1),
                    EndDate = new DateOnly(2022, 8, 1),
                    Description = "An open-source real-time metrics dashboard with 2k+ GitHub stars.",
                    Technologies = ["Blazor", "C#", "SignalR", "PostgreSQL"]
                }
            ],
            Certifications =
            [
                new Certification
                {
                    Id = Guid.NewGuid(),
                    Name = "Azure Solutions Architect Expert",
                    Issuer = "Microsoft",
                    IssueDate = new DateOnly(2023, 4, 15),
                    ExpiryDate = new DateOnly(2025, 4, 15),
                    CredentialId = "AZ-305-12345"
                }
            ],
            Languages =
            [
                new Language { Id = Guid.NewGuid(), Name = "English", Proficiency = "Native" },
                new Language { Id = Guid.NewGuid(), Name = "Spanish", Proficiency = "Conversational" }
            ],
            SectionOrder = ["Experience", "Education", "Skills", "Projects", "Certifications", "Languages"]
        };
    }
}
