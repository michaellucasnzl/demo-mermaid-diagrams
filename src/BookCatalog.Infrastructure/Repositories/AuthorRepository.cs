using BookCatalog.Application.Interfaces;
using BookCatalog.Domain.Entities;
using BookCatalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Repositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly AppDbContext _context;

    public AuthorRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Author>> GetAllAsync() =>
        await _context.Authors.Include(a => a.Books).OrderBy(a => a.LastName).ToListAsync();

    public async Task<Author?> GetByIdAsync(Guid id) =>
        await _context.Authors.Include(a => a.Books).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<bool> AnyAsync() =>
        await _context.Authors.AnyAsync();

    public async Task AddAsync(Author author) =>
        await _context.Authors.AddAsync(author);

    public async Task AddRangeAsync(IEnumerable<Author> authors) =>
        await _context.Authors.AddRangeAsync(authors);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
