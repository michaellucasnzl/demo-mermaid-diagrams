using BookCatalog.Application.DTOs;
using BookCatalog.Domain.Entities;

namespace BookCatalog.Application.Interfaces;

public interface ICatalogService
{
    Task SeedAsync();
    Task<string> GetCatalogHtmlAsync();
    Task<IEnumerable<Author>> GetAuthorsAsync();
    Task<IEnumerable<Book>> GetBooksAsync();
    Task<Author> AddAuthorAsync(AddAuthorRequest request);
    Task<Book> AddBookAsync(AddBookRequest request);
    Task<BookReview> AddReviewAsync(Guid bookId, AddReviewRequest request);
}
