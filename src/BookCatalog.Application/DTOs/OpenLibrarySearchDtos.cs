using System.Text.Json.Serialization;

namespace BookCatalog.Application.DTOs;

// ── Author search  (search/authors.json?q=...) ────────────────────────────

public class OpenLibraryAuthorSearchResponse
{
    [JsonPropertyName("numFound")]
    public int NumFound { get; set; }

    [JsonPropertyName("docs")]
    public List<OpenLibraryAuthorSearchItem> Docs { get; set; } = [];
}

public class OpenLibraryAuthorSearchItem
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;   // e.g. "OL26320A"

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("birth_date")]
    public string? BirthDate { get; set; }

    [JsonPropertyName("death_date")]
    public string? DeathDate { get; set; }

    [JsonPropertyName("top_work")]
    public string? TopWork { get; set; }

    [JsonPropertyName("work_count")]
    public int WorkCount { get; set; }
}

// ── Book search  (search.json?q=...&fields=...) ───────────────────────────

public class OpenLibraryBookSearchResponse
{
    [JsonPropertyName("numFound")]
    public int NumFound { get; set; }

    [JsonPropertyName("docs")]
    public List<OpenLibraryBookSearchItem> Docs { get; set; } = [];
}

public class OpenLibraryBookSearchItem
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;   // e.g. "/works/OL27482W"

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("author_name")]
    public List<string>? AuthorName { get; set; }

    [JsonPropertyName("first_publish_year")]
    public int? FirstPublishYear { get; set; }

    [JsonPropertyName("number_of_pages_median")]
    public int? NumberOfPagesMedian { get; set; }

    [JsonPropertyName("publisher")]
    public List<string>? Publisher { get; set; }

    [JsonPropertyName("isbn")]
    public List<string>? Isbn { get; set; }

    [JsonPropertyName("subject")]
    public List<string>? Subject { get; set; }

    /// <summary>Returns just the bare OL key, e.g. "OL27482W" from "/works/OL27482W".</summary>
    public string BareKey => Key.StartsWith("/works/") ? Key["/works/".Length..] : Key;
}
