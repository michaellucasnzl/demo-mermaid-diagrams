using System.Text.Json.Serialization;

namespace BookCatalog.Application.DTOs;

public class OpenLibraryBookDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public OpenLibraryValueOrString? Description { get; set; }

    [JsonPropertyName("covers")]
    public List<int>? Covers { get; set; }

    public string GetDescription() => Description?.Value ?? string.Empty;

    public string? GetCoverUrl()
    {
        if (Covers is { Count: > 0 } && Covers[0] > 0)
            return $"https://covers.openlibrary.org/b/id/{Covers[0]}-L.jpg";
        return null;
    }
}
