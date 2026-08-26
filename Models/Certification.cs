namespace REM.Models;

public class Certification
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string Issuer { get; set; } = "";

    public DateOnly IssueDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string CredentialId { get; set; } = "";

    public string Url { get; set; } = "";
}
