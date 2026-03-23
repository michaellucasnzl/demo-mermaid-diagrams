using BookCatalog.Application.DTOs;
using BookCatalog.Application.Interfaces;
using BookCatalog.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BookCatalog.Application.Services;

public class CatalogService(
    IDatabaseSeeder seeder,
    IAuthorRepository authorRepository,
    IBookRepository bookRepository,
    IGenreRepository genreRepository,
    IPublisherRepository publisherRepository,
    ILogger<CatalogService> logger) : ICatalogService
{
    public async Task SeedAsync()
    {
        logger.LogInformation("Checking if database seeding is needed...");
        await seeder.SeedAsync();


    }

    public async Task<string> GetCatalogHtmlAsync()
    {
        var authors = (await authorRepository.GetAllAsync()).ToList();
        var books = (await bookRepository.GetAllWithDetailsAsync())
            .OrderBy(b => b.Author.LastName).ThenBy(b => b.PublishedYear)
            .ToList();
        return BuildHtml(authors, books);
    }

    public Task<IEnumerable<Author>> GetAuthorsAsync() =>
        authorRepository.GetAllAsync();

    public Task<IEnumerable<Book>> GetBooksAsync() =>
        bookRepository.GetAllWithDetailsAsync();

    public async Task<Author> AddAuthorAsync(AddAuthorRequest request)
    {
        var author = new Author
        {
            Id             = Guid.NewGuid(),
            FirstName      = request.FirstName,
            LastName       = request.LastName,
            BirthDate      = new DateOnly(request.BirthYear, 1, 1),
            Nationality    = request.Nationality,
            Biography      = request.Biography ?? string.Empty,
            OpenLibraryKey = string.Empty
        };
        await authorRepository.AddAsync(author);
        await authorRepository.SaveChangesAsync();
        logger.LogInformation("Added author: {FullName}", author.FullName);
        return author;
    }

    public async Task<Book> AddBookAsync(AddBookRequest request)
    {
        _ = await authorRepository.GetByIdAsync(request.AuthorId)
            ?? throw new KeyNotFoundException($"Author '{request.AuthorId}' not found.");

        var publisher = await publisherRepository.GetOrCreateAsync(request.PublisherName);

        var bookId = Guid.NewGuid();
        var book = new Book
        {
            Id             = bookId,
            Title          = request.Title,
            ISBN           = request.ISBN,
            PublishedYear  = request.PublishedYear,
            PageCount      = request.PageCount,
            AuthorId       = request.AuthorId,
            PublisherId    = publisher.Id,
            Description    = string.Empty,
            OpenLibraryKey = string.Empty
        };

        foreach (var genreName in request.Genres)
        {
            var genre = await genreRepository.GetOrCreateAsync(genreName);
            book.BookGenres.Add(new BookGenre { BookId = bookId, GenreId = genre.Id });
        }

        await bookRepository.AddAsync(book);
        await bookRepository.SaveChangesAsync();
        logger.LogInformation("Added book: {Title}", book.Title);
        return book;
    }

    public async Task<BookReview> AddReviewAsync(Guid bookId, AddReviewRequest request)
    {
        _ = await bookRepository.GetByIdAsync(bookId)
            ?? throw new KeyNotFoundException($"Book '{bookId}' not found.");

        var review = new BookReview
        {
            Id           = Guid.NewGuid(),
            BookId       = bookId,
            ReviewerName = request.ReviewerName,
            Rating       = request.Rating,
            ReviewText   = request.ReviewText,
            CreatedDate  = DateTime.UtcNow
        };
        await bookRepository.AddReviewAsync(review);
        await bookRepository.SaveChangesAsync();
        logger.LogInformation("Added review for book {BookId} by {Reviewer}", bookId, request.ReviewerName);
        return review;
    }

    private static string BuildHtml(List<Author> authors, List<Book> books)
    {
        var sb = new StringBuilder();
        var generatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("  <title>Book Catalog</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine(EmbeddedCss);
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <header>");
        sb.AppendLine("    <h1>&#128218; Book Catalog</h1>");
        sb.AppendLine($"    <p class=\"generated\">Generated at {generatedAt}</p>");
        sb.AppendLine("  </header>");
        sb.AppendLine("  <main>");

        // --- Authors section ---
        sb.AppendLine("    <section>");
        sb.AppendLine("      <h2>Authors</h2>");
        sb.AppendLine("      <table>");
        sb.AppendLine("        <thead><tr><th>Name</th><th>Born</th><th>Died</th><th>Nationality</th><th>Books</th><th>Biography</th></tr></thead>");
        sb.AppendLine("        <tbody>");
        foreach (var author in authors)
        {
            var bio = author.Biography.Length > 300 ? author.Biography[..300] + "\u2026" : author.Biography;
            sb.AppendLine($"          <tr>");
            sb.AppendLine($"            <td class=\"name\">{H(author.FullName)}</td>");
            sb.AppendLine($"            <td>{author.BirthDate.Year}</td>");
            sb.AppendLine($"            <td>{(author.DeathDate.HasValue ? author.DeathDate.Value.Year.ToString() : "\u2014")}</td>");
            sb.AppendLine($"            <td>{H(author.Nationality)}</td>");
            sb.AppendLine($"            <td class=\"center\">{author.Books.Count}</td>");
            sb.AppendLine($"            <td class=\"bio\">{H(bio)}</td>");
            sb.AppendLine($"          </tr>");
        }
        sb.AppendLine("        </tbody>");
        sb.AppendLine("      </table>");
        sb.AppendLine("    </section>");

        // --- Books section ---
        sb.AppendLine("    <section>");
        sb.AppendLine("      <h2>Books</h2>");
        sb.AppendLine("      <table>");
        sb.AppendLine("        <thead><tr><th>Cover</th><th>Title</th><th>Author</th><th>Publisher</th><th>Year</th><th>Pages</th><th>ISBN</th><th>Genres</th><th>Rating</th></tr></thead>");
        sb.AppendLine("        <tbody>");
        foreach (var book in books)
        {
            var avgRating = book.Reviews.Any() ? book.Reviews.Average(r => r.Rating) : 0;
            var stars = avgRating > 0 ? RenderStars(avgRating) : "<span class=\"muted\">\u2014</span>";
            var genres = string.Join(", ", book.BookGenres.Select(bg => bg.Genre.Name));
            var coverHtml = !string.IsNullOrEmpty(book.CoverUrl)
                ? $"<img src=\"{H(book.CoverUrl)}\" alt=\"Cover\" loading=\"lazy\">"
                : "<span class=\"no-cover\">&#128218;</span>";

            sb.AppendLine($"          <tr>");
            sb.AppendLine($"            <td class=\"cover\">{coverHtml}</td>");
            sb.AppendLine($"            <td class=\"name\">{H(book.Title)}</td>");
            sb.AppendLine($"            <td>{H(book.Author.FullName)}</td>");
            sb.AppendLine($"            <td>{H(book.Publisher.Name)}</td>");
            sb.AppendLine($"            <td>{book.PublishedYear}</td>");
            sb.AppendLine($"            <td class=\"center\">{book.PageCount}</td>");
            sb.AppendLine($"            <td class=\"mono\">{H(book.ISBN)}</td>");
            sb.AppendLine($"            <td>{H(genres)}</td>");
            sb.AppendLine($"            <td class=\"rating\">{stars}</td>");
            sb.AppendLine($"          </tr>");
            if (book.Reviews.Any())
            {
                sb.AppendLine($"          <tr class=\"reviews-row\">");
                sb.AppendLine($"            <td colspan=\"9\">");
                sb.AppendLine($"              <div class=\"reviews\">");
                foreach (var review in book.Reviews.OrderByDescending(r => r.Rating))
                {
                    sb.AppendLine($"                <div class=\"review\">");
                    sb.AppendLine($"                  <span class=\"reviewer\">{H(review.ReviewerName)}</span>");
                    sb.AppendLine($"                  <span class=\"review-stars\">{RenderStars(review.Rating)}</span>");
                    sb.AppendLine($"                  <span class=\"review-text\">{H(review.ReviewText)}</span>");
                    sb.AppendLine($"                </div>");
                }
                sb.AppendLine($"              </div>");
                sb.AppendLine($"            </td>");
                sb.AppendLine($"          </tr>");
            }
        }
        sb.AppendLine("        </tbody>");
        sb.AppendLine("      </table>");
        sb.AppendLine("    </section>");

        sb.AppendLine("  </main>");
        sb.AppendLine("  <footer><p>Data sourced from <a href=\"https://openlibrary.org\">Open Library</a>. Generated by BookCatalog demo app.</p></footer>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    private static string RenderStars(double rating)
    {
        var full = (int)Math.Round(rating);
        var stars = new StringBuilder();
        for (int i = 1; i <= 5; i++)
            stars.Append(i <= full ? "&#9733;" : "&#9734;");
        return $"<span class=\"stars\" title=\"{rating:F1}\">{stars}</span>";
    }

    private static string H(string? s) =>
        System.Net.WebUtility.HtmlEncode(s ?? string.Empty);

    private const string EmbeddedCss = """
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: #0f1117;
            color: #e2e8f0;
            min-height: 100vh;
        }

        header {
            background: linear-gradient(135deg, #1a1f2e 0%, #2d3748 100%);
            padding: 2.5rem 2rem 2rem;
            border-bottom: 1px solid #2d3748;
        }

        header h1 {
            font-size: 2rem;
            font-weight: 700;
            color: #f7fafc;
            letter-spacing: -0.02em;
        }

        .generated {
            margin-top: 0.4rem;
            color: #718096;
            font-size: 0.85rem;
        }

        main { padding: 2rem; max-width: 1400px; margin: 0 auto; }

        section { margin-bottom: 3rem; }

        h2 {
            font-size: 1.25rem;
            font-weight: 600;
            color: #a0aec0;
            text-transform: uppercase;
            letter-spacing: 0.08em;
            margin-bottom: 1rem;
            padding-bottom: 0.5rem;
            border-bottom: 1px solid #2d3748;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            font-size: 0.9rem;
        }

        thead tr { background: #1a1f2e; }

        thead th {
            padding: 0.75rem 1rem;
            text-align: left;
            font-weight: 600;
            color: #a0aec0;
            font-size: 0.8rem;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            border-bottom: 2px solid #2d3748;
        }

        tbody tr {
            border-bottom: 1px solid #1a1f2e;
            transition: background 0.15s;
        }

        tbody tr:hover { background: #1a1f2e; }

        tbody tr.reviews-row {
            background: #0d1018;
            border-bottom: 2px solid #2d3748;
        }

        tbody tr.reviews-row:hover { background: #0d1018; }

        td {
            padding: 0.75rem 1rem;
            vertical-align: top;
            color: #cbd5e0;
        }

        td.name { font-weight: 600; color: #f7fafc; }
        td.center { text-align: center; }
        td.mono { font-family: 'SFMono-Regular', Consolas, monospace; font-size: 0.8rem; color: #718096; }
        td.bio { font-size: 0.82rem; color: #718096; max-width: 400px; line-height: 1.5; }
        td.cover { width: 60px; padding: 0.4rem 0.75rem; }
        td.cover img { width: 48px; height: auto; border-radius: 3px; display: block; }
        td.cover .no-cover { font-size: 2rem; display: block; text-align: center; }
        td.rating { white-space: nowrap; }

        .stars { color: #f6ad55; letter-spacing: 0.05em; }

        .reviews {
            display: flex;
            flex-direction: column;
            gap: 0.6rem;
            padding: 0.75rem 1rem;
        }

        .review {
            display: flex;
            align-items: baseline;
            gap: 0.75rem;
            font-size: 0.82rem;
        }

        .reviewer { font-weight: 600; color: #a0aec0; min-width: 120px; }
        .review-stars { color: #f6ad55; white-space: nowrap; }
        .review-text { color: #718096; line-height: 1.5; }
        .muted { color: #4a5568; }

        footer {
            text-align: center;
            padding: 2rem;
            color: #4a5568;
            font-size: 0.8rem;
            border-top: 1px solid #1a1f2e;
        }

        footer a { color: #667eea; text-decoration: none; }
        footer a:hover { text-decoration: underline; }
        """;
}
