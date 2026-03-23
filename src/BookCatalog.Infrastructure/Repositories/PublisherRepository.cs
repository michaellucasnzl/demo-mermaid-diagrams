using BookCatalog.Application.Interfaces;
using BookCatalog.Domain.Entities;
using BookCatalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Repositories;

public class PublisherRepository(AppDbContext context) : IPublisherRepository
{
    public async Task<Publisher?> GetByNameAsync(string name) =>
        await context.Publishers.FirstOrDefaultAsync(p =>
            p.Name.ToLower() == name.ToLower());

    public async Task<Publisher> GetOrCreateAsync(string name)
    {
        var existing = await GetByNameAsync(name);
        if (existing != null) return existing;

        var publisher = new Publisher
        {
            Id          = Guid.NewGuid(),
            Name        = name,
            FoundedYear = 0,
            Country     = "Unknown"
        };
        context.Publishers.Add(publisher);
        await context.SaveChangesAsync();
        return publisher;
    }

    public async Task<IEnumerable<Publisher>> GetAllAsync() =>
        await context.Publishers.OrderBy(p => p.Name).ToListAsync();
}
