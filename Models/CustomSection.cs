namespace REM.Models;

public class CustomSection
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Title { get; set; } = "Custom Section";

    public List<CustomEntry> Items { get; set; } = [];
}
