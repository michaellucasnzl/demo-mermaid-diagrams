using BookCatalog.Application.DTOs;

namespace BookCatalog.Application.Interfaces;

public interface IOpenLibraryService
{
    Task<OpenLibraryAuthorDto?> GetAuthorAsync(string openLibraryKey);
    Task<OpenLibraryBookDto?> GetBookAsync(string openLibraryKey);
    Task<List<OpenLibraryAuthorSearchItem>> SearchAuthorsAsync(string query, int limit = 10);
    Task<List<OpenLibraryBookSearchItem>> SearchBooksAsync(string query, int limit = 10);
}
