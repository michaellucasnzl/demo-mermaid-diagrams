using BookCatalog.Domain.Entities;

namespace BookCatalog.Application.Interfaces;

public interface IPublisherRepository
{
    Task<Publisher?> GetByNameAsync(string name);
    Task<Publisher> GetOrCreateAsync(string name);
    Task<IEnumerable<Publisher>> GetAllAsync();
}
