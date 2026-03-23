using System.Text.Json.Serialization;

namespace BookCatalog.Application.DTOs;

public class OpenLibraryAuthorDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("bio")]
    public OpenLibraryValueOrString? Bio { get; set; }

    [JsonPropertyName("birth_date")]
    public string? BirthDate { get; set; }

    [JsonPropertyName("death_date")]
    public string? DeathDate { get; set; }

    [JsonPropertyName("wikipedia")]
    public string? Wikipedia { get; set; }

    public string GetBiography() => Bio?.Value ?? string.Empty;
}

public class OpenLibraryValueOrString
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
