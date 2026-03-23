using BookCatalog.Domain.Entities;

namespace BookCatalog.Application.Interfaces;

public interface IAuthorRepository
{
    Task<IEnumerable<Author>> GetAllAsync();
    Task<Author?> GetByIdAsync(Guid id);
    Task<bool> AnyAsync();
    Task AddAsync(Author author);
    Task AddRangeAsync(IEnumerable<Author> authors);
    Task SaveChangesAsync();
}
