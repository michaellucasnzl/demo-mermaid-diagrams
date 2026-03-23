using BookCatalog.Application.Interfaces;
using BookCatalog.Domain.Entities;
using BookCatalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Book>> GetAllWithDetailsAsync() =>
        await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
            .Include(b => b.Reviews)
            .OrderBy(b => b.Author.LastName).ThenBy(b => b.PublishedYear)
            .ToListAsync();

    public async Task<Book?> GetByIdAsync(Guid id) =>
        await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Publisher)
            .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
            .Include(b => b.Reviews)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task AddAsync(Book book) =>
        await _context.Books.AddAsync(book);

    public async Task AddRangeAsync(IEnumerable<Book> books) =>
        await _context.Books.AddRangeAsync(books);

    public async Task AddReviewAsync(BookReview review) =>
        await _context.BookReviews.AddAsync(review);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
