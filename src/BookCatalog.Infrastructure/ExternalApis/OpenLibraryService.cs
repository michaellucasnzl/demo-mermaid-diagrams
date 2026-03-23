using System.Net.Http.Json;
using BookCatalog.Application.DTOs;
using BookCatalog.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookCatalog.Infrastructure.ExternalApis;

public class OpenLibraryService : IOpenLibraryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenLibraryService> _logger;

    public OpenLibraryService(HttpClient httpClient, ILogger<OpenLibraryService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<OpenLibraryAuthorDto?> GetAuthorAsync(string openLibraryKey)
    {
        try
        {
            var url = $"https://openlibrary.org/authors/{openLibraryKey}.json";
            _logger.LogInformation("Fetching author data from Open Library: {Url}", url);
            var result = await _httpClient.GetFromJsonAsync<OpenLibraryAuthorDto>(url);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to fetch author {Key} from Open Library: {Message}", openLibraryKey, ex.Message);
            return null;
        }
    }

    public async Task<OpenLibraryBookDto?> GetBookAsync(string openLibraryKey)
    {
        try
        {
            var url = $"https://openlibrary.org/works/{openLibraryKey}.json";
            _logger.LogInformation("Fetching book data from Open Library: {Url}", url);
            var result = await _httpClient.GetFromJsonAsync<OpenLibraryBookDto>(url);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to fetch book {Key} from Open Library: {Message}", openLibraryKey, ex.Message);
            return null;
        }
    }

    public async Task<List<OpenLibraryAuthorSearchItem>> SearchAuthorsAsync(string query, int limit = 10)
    {
        try
        {
            var url = $"https://openlibrary.org/search/authors.json?q={Uri.EscapeDataString(query)}&limit={limit}";
            _logger.LogInformation("Searching authors on Open Library: {Query}", query);
            var result = await _httpClient.GetFromJsonAsync<OpenLibraryAuthorSearchResponse>(url);
            return result?.Docs ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Author search failed for '{Query}': {Message}", query, ex.Message);
            return [];
        }
    }

    public async Task<List<OpenLibraryBookSearchItem>> SearchBooksAsync(string query, int limit = 10)
    {
        try
        {
            var fields = "key,title,author_name,first_publish_year,number_of_pages_median,publisher,isbn,subject";
            var url = $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(query)}&fields={fields}&limit={limit}";
            _logger.LogInformation("Searching books on Open Library: {Query}", query);
            var result = await _httpClient.GetFromJsonAsync<OpenLibraryBookSearchResponse>(url);
            return result?.Docs ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Book search failed for '{Query}': {Message}", query, ex.Message);
            return [];
        }
    }
}
