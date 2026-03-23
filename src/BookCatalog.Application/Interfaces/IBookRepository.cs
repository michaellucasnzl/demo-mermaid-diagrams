using BookCatalog.Domain.Entities;

namespace BookCatalog.Application.Interfaces;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllWithDetailsAsync();
    Task<Book?> GetByIdAsync(Guid id);
    Task AddAsync(Book book);
    Task AddRangeAsync(IEnumerable<Book> books);
    Task AddReviewAsync(BookReview review);
    Task SaveChangesAsync();
}
