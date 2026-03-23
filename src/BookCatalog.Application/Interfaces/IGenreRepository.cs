using BookCatalog.Domain.Entities;

namespace BookCatalog.Application.Interfaces;

public interface IGenreRepository
{
    Task<Genre?> GetByNameAsync(string name);
    Task<Genre> GetOrCreateAsync(string name);
}
