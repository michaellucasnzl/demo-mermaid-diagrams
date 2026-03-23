using BookCatalog.Application.Interfaces;
using BookCatalog.Domain.Entities;
using BookCatalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Repositories;

public class GenreRepository(AppDbContext context) : IGenreRepository
{
    public async Task<Genre?> GetByNameAsync(string name) =>
        await context.Genres.FirstOrDefaultAsync(g =>
            g.Name.ToLower() == name.ToLower());

    public async Task<Genre> GetOrCreateAsync(string name)
    {
        var existing = await GetByNameAsync(name);
        if (existing != null) return existing;

        var genre = new Genre { Id = Guid.NewGuid(), Name = name, Description = string.Empty };
        context.Genres.Add(genre);
        await context.SaveChangesAsync();
        return genre;
    }
}
