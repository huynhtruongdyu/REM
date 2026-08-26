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

    public ResumeDocument Import(string json)
        => JsonSerializer.Deserialize<ResumeDocument>(json)
            ?? throw new InvalidOperationException("Invalid resume file.");
}
