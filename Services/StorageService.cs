using System.Text.Json;
using REM.Models;
using Microsoft.JSInterop;

namespace REM.Services;

public class StorageService
{
    private const string Key = "rem-resume";

    private readonly IJSRuntime _js;

    public StorageService(IJSRuntime js)
    {
        _js = js;
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public async Task SaveAsync(ResumeDocument doc)
        => await _js.InvokeVoidAsync("storage.save", Key, JsonSerializer.Serialize(doc, Options));

    public async Task<ResumeDocument?> LoadAsync()
    {
        var json = await _js.InvokeAsync<string?>("storage.load", Key);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ResumeDocument>(json);
    }

    public async Task ExportAsync(ResumeDocument doc)
        => await _js.InvokeVoidAsync("storage.downloadFile", "resume.json", JsonSerializer.Serialize(doc, Options));

    public ResumeDocument Import(string json)
        => JsonSerializer.Deserialize<ResumeDocument>(json)
            ?? throw new InvalidOperationException("Invalid resume file.");
}
