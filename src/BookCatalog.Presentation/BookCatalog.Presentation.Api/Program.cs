using BookCatalog.Application.DTOs;
using BookCatalog.Application.Interfaces;
using BookCatalog.Infrastructure;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=bookcatalog;Username=postgres;Password=postgres";

builder.Services.AddInfrastructure(connectionString);
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Apply EF migrations and seed on startup
await DependencyInjection.ApplyMigrationsAsync(app.Services);
using (var scope = app.Services.CreateScope())
{
    var catalog = scope.ServiceProvider.GetRequiredService<ICatalogService>();
    await catalog.SeedAsync();
}

// GET / — serve the live HTML catalog page
app.MapGet("/", async (ICatalogService svc) =>
    Results.Content(await svc.GetCatalogHtmlAsync(), "text/html"));

// GET /api/authors
app.MapGet("/api/authors", async (ICatalogService svc) =>
{
    var authors = await svc.GetAuthorsAsync();
    return Results.Ok(authors.Select(a => new
    {
        id          = a.Id,
        fullName    = a.FullName,
        firstName   = a.FirstName,
        lastName    = a.LastName,
        birthYear   = a.BirthDate.Year,
        deathYear   = a.DeathDate?.Year,
        nationality = a.Nationality,
        biography   = a.Biography,
        bookCount   = a.Books.Count
    }));
});

// GET /api/books
app.MapGet("/api/books", async (ICatalogService svc) =>
{
    var books = await svc.GetBooksAsync();
    return Results.Ok(books.Select(b => new
    {
        id            = b.Id,
        title         = b.Title,
        authorId      = b.AuthorId,
        authorName    = b.Author.FullName,
        publisher     = b.Publisher.Name,
        isbn          = b.ISBN,
        publishedYear = b.PublishedYear,
        pageCount     = b.PageCount,
        coverUrl      = b.CoverUrl,
        genres        = b.BookGenres.Select(bg => bg.Genre.Name).ToList(),
        averageRating = b.Reviews.Any() ? (double?)b.Reviews.Average(r => r.Rating) : null,
        reviewCount   = b.Reviews.Count
    }));
});

// POST /api/authors
app.MapPost("/api/authors", async (AddAuthorRequest req, ICatalogService svc) =>
{
    var author = await svc.AddAuthorAsync(req);
    return Results.Created($"/api/authors/{author.Id}",
        new { id = author.Id, fullName = author.FullName });
});

// POST /api/books
app.MapPost("/api/books", async (AddBookRequest req, ICatalogService svc) =>
{
    try
    {
        var book = await svc.AddBookAsync(req);
        return Results.Created($"/api/books/{book.Id}",
            new { id = book.Id, title = book.Title });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

// POST /api/reviews/{bookId}
app.MapPost("/api/reviews/{bookId:guid}", async (Guid bookId, AddReviewRequest req, ICatalogService svc) =>
{
    try
    {
        var review = await svc.AddReviewAsync(bookId, req);
        return Results.Created($"/api/reviews/{review.Id}", new { id = review.Id });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

// POST /api/catalog/seed — re-seed if the DB is empty
app.MapPost("/api/catalog/seed", async (ICatalogService svc) =>
{
    await svc.SeedAsync();
    return Results.Ok(new { message = "Seed check complete." });
});

// GET /api/search/authors?q=tolkien
app.MapGet("/api/search/authors", async (string q, IOpenLibraryService olSvc) =>
{
    if (string.IsNullOrWhiteSpace(q))
        return Results.BadRequest(new { error = "q parameter is required." });

    var results = await olSvc.SearchAuthorsAsync(q.Trim());
    return Results.Ok(results.Select(a => new
    {
        key       = a.Key,
        name      = a.Name,
        birthDate = a.BirthDate,
        deathDate = a.DeathDate,
        topWork   = a.TopWork,
        workCount = a.WorkCount
    }));
});

// GET /api/search/books?q=hobbit
app.MapGet("/api/search/books", async (string q, IOpenLibraryService olSvc) =>
{
    if (string.IsNullOrWhiteSpace(q))
        return Results.BadRequest(new { error = "q parameter is required." });

    var results = await olSvc.SearchBooksAsync(q.Trim());
    return Results.Ok(results.Select(b => new
    {
        key             = b.BareKey,
        title           = b.Title,
        authorName      = b.AuthorName?.FirstOrDefault(),
        firstPublishYear = b.FirstPublishYear,
        pageCount       = b.NumberOfPagesMedian,
        publisher       = b.Publisher?.FirstOrDefault(),
        isbn            = b.Isbn?.FirstOrDefault(),
        subjects        = b.Subject?.Take(5).ToList()
    }));
});

app.Run();
