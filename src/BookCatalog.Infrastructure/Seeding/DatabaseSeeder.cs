using BookCatalog.Application.Interfaces;
using BookCatalog.Domain.Entities;
using BookCatalog.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace BookCatalog.Infrastructure.Seeding;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly IOpenLibraryService _openLibraryService;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext context, IOpenLibraryService openLibraryService, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _openLibraryService = openLibraryService;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (_context.Authors.Any())
        {
            _logger.LogInformation("Database already seeded. Skipping.");
            return;
        }

        _logger.LogInformation("Seeding database...");

        // --- Publishers ---
        var allenUnwin = new Publisher { Id = Guid.NewGuid(), Name = "Allen & Unwin", FoundedYear = 1914, Country = "United Kingdom", Website = "https://www.allenandunwin.com" };
        var geoffreyBles = new Publisher { Id = Guid.NewGuid(), Name = "Geoffrey Bles", FoundedYear = 1923, Country = "United Kingdom", Website = null };
        var puffinBooks = new Publisher { Id = Guid.NewGuid(), Name = "Puffin Books", FoundedYear = 1941, Country = "United Kingdom", Website = "https://www.puffin.co.uk" };

        await _context.Publishers.AddRangeAsync(allenUnwin, geoffreyBles, puffinBooks);

        // --- Genres ---
        var fantasy = new Genre { Id = Guid.NewGuid(), Name = "Fantasy", Description = "Fiction involving magical and supernatural elements in an imaginary world." };
        var childrens = new Genre { Id = Guid.NewGuid(), Name = "Children's Literature", Description = "Books written for and marketed to children." };
        var christianFiction = new Genre { Id = Guid.NewGuid(), Name = "Christian Fiction", Description = "Fiction incorporating Christian worldview and themes." };
        var adventure = new Genre { Id = Guid.NewGuid(), Name = "Adventure", Description = "Stories featuring exciting journeys and perilous quests." };
        var satire = new Genre { Id = Guid.NewGuid(), Name = "Satire", Description = "Using humour and irony to critique society or human nature." };

        await _context.Genres.AddRangeAsync(fantasy, childrens, christianFiction, adventure, satire);

        // --- Authors (with Open Library enrichment) ---
        var tolkien = await BuildAuthorAsync("OL26320A", "J.R.R.", "Tolkien",
            new DateOnly(1892, 1, 3), new DateOnly(1973, 9, 2), "British",
            "John Ronald Reuel Tolkien was an English writer, poet, and academic best known for The Hobbit and The Lord of the Rings.");

        var lewis = await BuildAuthorAsync("OL18319A", "C.S.", "Lewis",
            new DateOnly(1898, 11, 29), new DateOnly(1963, 11, 22), "British",
            "Clive Staples Lewis was a British writer, lay theologian, and academic best known for The Chronicles of Narnia.");

        var dahl = await BuildAuthorAsync("OL29435A", "Roald", "Dahl",
            new DateOnly(1916, 9, 13), new DateOnly(1990, 11, 23), "British",
            "Roald Dahl was a British novelist, short story writer, and screenwriter known for his inventive and darkly comic tales.");

        await _context.Authors.AddRangeAsync(tolkien, lewis, dahl);

        // --- Books ---
        var theHobbit = BuildBook("OL27516W", "The Hobbit", "9780547928227", 1937, 310, tolkien.Id, allenUnwin.Id,
            "Bilbo Baggins, a hobbit who enjoys a comfortable life, is swept into an epic quest to reclaim the lost Dwarf Kingdom of Erebor.");

        var fellowship = BuildBook("OL27516W2", "The Fellowship of the Ring", "9780547928210", 1954, 423, tolkien.Id, allenUnwin.Id,
            "The first part of The Lord of the Rings. Frodo Baggins inherits the One Ring and begins his journey to destroy it.");

        var twoTowers = BuildBook("OL27516W3", "The Two Towers", "9780547928203", 1954, 352, tolkien.Id, allenUnwin.Id,
            "The second part of The Lord of the Rings follows the Fellowship after it has been broken.");

        var returnOfKing = BuildBook("OL27516W4", "The Return of the King", "9780547928197", 1955, 416, tolkien.Id, allenUnwin.Id,
            "The final part of The Lord of the Rings. The War of the Ring reaches its climax and Frodo achieves his quest.");

        var lionWitch = BuildBook("OL7958833W", "The Lion, the Witch and the Wardrobe", "9780064404990", 1950, 208, lewis.Id, geoffreyBles.Id,
            "Four children discover a magical wardrobe that leads to the land of Narnia, ruled by the White Witch.");

        var princeCaspian = BuildBook("OL7958834W", "Prince Caspian", "9780064471060", 1951, 195, lewis.Id, geoffreyBles.Id,
            "The Pevensie children return to Narnia to help Prince Caspian reclaim his rightful throne.");

        var screwtape = BuildBook("OL7958835W", "The Screwtape Letters", "9780060652937", 1942, 209, lewis.Id, geoffreyBles.Id,
            "A senior demon instructs a junior tempter on the best ways to corrupt a human soul — a satirical Christian allegory.");

        var charlie = BuildBook("OL45804W", "Charlie and the Chocolate Factory", "9780142410318", 1964, 176, dahl.Id, puffinBooks.Id,
            "Young Charlie Bucket wins a golden ticket to visit the mysterious factory of eccentric chocolatier Willy Wonka.");

        var matilda = BuildBook("OL1975468W", "Matilda", "9780142410370", 1988, 240, dahl.Id, puffinBooks.Id,
            "A brilliant young girl named Matilda discovers she has telekinetic powers and uses them to stand up to her terrible parents and headmistress.");

        var jamesGiantPeach = BuildBook("OL45805W", "James and the Giant Peach", "9780142410325", 1961, 146, dahl.Id, puffinBooks.Id,
            "A young English boy named James escapes from his horrible aunts by travelling across the ocean inside a giant peach.");

        // Enrich descriptions from Open Library
        await EnrichBookFromApiAsync(theHobbit, "OL27516W");
        await EnrichBookFromApiAsync(lionWitch, "OL7958833W");
        await EnrichBookFromApiAsync(charlie, "OL45804W");
        await EnrichBookFromApiAsync(matilda, "OL1975468W");

        await _context.Books.AddRangeAsync(theHobbit, fellowship, twoTowers, returnOfKing,
            lionWitch, princeCaspian, screwtape, charlie, matilda, jamesGiantPeach);

        // --- Book-Genre assignments ---
        var bookGenres = new List<BookGenre>
        {
            new() { BookId = theHobbit.Id, GenreId = fantasy.Id },
            new() { BookId = theHobbit.Id, GenreId = adventure.Id },
            new() { BookId = theHobbit.Id, GenreId = childrens.Id },
            new() { BookId = fellowship.Id, GenreId = fantasy.Id },
            new() { BookId = fellowship.Id, GenreId = adventure.Id },
            new() { BookId = twoTowers.Id, GenreId = fantasy.Id },
            new() { BookId = twoTowers.Id, GenreId = adventure.Id },
            new() { BookId = returnOfKing.Id, GenreId = fantasy.Id },
            new() { BookId = returnOfKing.Id, GenreId = adventure.Id },
            new() { BookId = lionWitch.Id, GenreId = fantasy.Id },
            new() { BookId = lionWitch.Id, GenreId = childrens.Id },
            new() { BookId = lionWitch.Id, GenreId = christianFiction.Id },
            new() { BookId = princeCaspian.Id, GenreId = fantasy.Id },
            new() { BookId = princeCaspian.Id, GenreId = childrens.Id },
            new() { BookId = screwtape.Id, GenreId = christianFiction.Id },
            new() { BookId = screwtape.Id, GenreId = satire.Id },
            new() { BookId = charlie.Id, GenreId = childrens.Id },
            new() { BookId = charlie.Id, GenreId = fantasy.Id },
            new() { BookId = matilda.Id, GenreId = childrens.Id },
            new() { BookId = matilda.Id, GenreId = satire.Id },
            new() { BookId = jamesGiantPeach.Id, GenreId = childrens.Id },
            new() { BookId = jamesGiantPeach.Id, GenreId = fantasy.Id },
            new() { BookId = jamesGiantPeach.Id, GenreId = adventure.Id },
        };

        await _context.BookGenres.AddRangeAsync(bookGenres);

        // --- Reviews ---
        var reviews = BuildReviews(theHobbit.Id, fellowship.Id, twoTowers.Id, returnOfKing.Id,
            lionWitch.Id, princeCaspian.Id, screwtape.Id, charlie.Id, matilda.Id, jamesGiantPeach.Id);

        await _context.BookReviews.AddRangeAsync(reviews);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Database seeded successfully with {Count} books.", 10);
    }

    private async Task<Author> BuildAuthorAsync(string olKey, string firstName, string lastName,
        DateOnly birthDate, DateOnly? deathDate, string nationality, string fallbackBio)
    {
        var author = new Author
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            BirthDate = birthDate,
            DeathDate = deathDate,
            Nationality = nationality,
            OpenLibraryKey = olKey,
            Biography = fallbackBio
        };

        var apiData = await _openLibraryService.GetAuthorAsync(olKey);
        if (apiData is not null && !string.IsNullOrWhiteSpace(apiData.GetBiography()))
        {
            var bio = apiData.GetBiography();
            author.Biography = bio.Length > 4000 ? bio[..4000] : bio;
            _logger.LogInformation("Enriched biography for {Name} from Open Library.", author.FullName);
        }

        return author;
    }

    private static Book BuildBook(string olKey, string title, string isbn, int year,
        int pages, Guid authorId, Guid publisherId, string fallbackDescription)
        => new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            ISBN = isbn,
            PublishedYear = year,
            PageCount = pages,
            AuthorId = authorId,
            PublisherId = publisherId,
            OpenLibraryKey = olKey,
            Description = fallbackDescription
        };

    private async Task EnrichBookFromApiAsync(Book book, string olKey)
    {
        var apiData = await _openLibraryService.GetBookAsync(olKey);
        if (apiData is null) return;

        var desc = apiData.GetDescription();
        if (!string.IsNullOrWhiteSpace(desc))
            book.Description = desc.Length > 4000 ? desc[..4000] : desc;

        var cover = apiData.GetCoverUrl();
        if (!string.IsNullOrWhiteSpace(cover))
            book.CoverUrl = cover;
    }

    private static List<BookReview> BuildReviews(params Guid[] bookIds)
    {
        var reviewData = new (string reviewer, int rating, string text)[]
        {
            ("Alice M.", 5, "An absolute classic! Tolkien's world-building is unmatched. I've read this three times and it just gets better."),
            ("James P.", 5, "The book that started my love of fantasy. Bilbo's journey feels both cosy and epic at the same time."),
            ("Sarah K.", 5, "Tolkien creates a world so rich and detailed you can almost smell the Shire. A masterpiece of storytelling."),
            ("Robert H.", 4, "A slow start but the payoff is extraordinary. The friendship between the Fellowship members is deeply moving."),
            ("Emily T.", 5, "The Two Towers is where Tolkien truly hits his stride. Rohan and Helm's Deep are breathtaking set pieces."),
            ("David L.", 4, "An epic conclusion to a monumental trilogy. The scouring of the Shire is a haunting final act."),
            ("Rachel W.", 5, "Lewis writes with such effortless magic. Every page of this book felt like pure wonder."),
            ("Michael B.", 5, "The Lion, the Witch and the Wardrobe is timeless. My children love it as much as I did at their age."),
            ("Laura G.", 4, "Prince Caspian is a darker, more mature Narnia story. The themes of faith and doubt are remarkably deep."),
            ("Tom N.", 5, "The Screwtape Letters is unlike anything I've read. Brilliant and chilling in equal measure."),
            ("Fiona C.", 5, "Dahl has an extraordinary gift for capturing the wonder of childhood. One of the greatest books ever written for children."),
            ("Peter O.", 5, "Willy Wonka is one of literature's great eccentric characters. Pure delight from start to finish."),
            ("Jennifer A.", 5, "Matilda is inspiring, funny, and occasionally heartbreaking. Miss Trunchbull is one of the best villains in children's fiction."),
            ("Chris D.", 5, "Dahl's language is so inventive and joyful. Matilda is a love letter to books and reading."),
            ("Helen F.", 5, "James and the Giant Peach is wonderfully surreal. Dahl's imagination knows no bounds."),
            ("Mark S.", 4, "A bizarre and brilliant adventure. The insect characters are surprisingly endearing."),
            ("Sophie R.", 5, "The Return of the King is a perfect ending. 'The grey rain-curtain of this world rolls back' still gives me chills."),
            ("Ben T.", 5, "Screwtape is pure genius — theology wrapped in wickedly clever satire. Deserves to be read by everyone."),
            ("Anna K.", 4, "Fellowship is a slow but rewarding read. The Shire chapters are some of the most comforting writing in literature."),
            ("George V.", 5, "The Two Towers shows Tolkien's mastery of epic scale balanced with intimate character moments."),
        };

        var reviews = new List<BookReview>();
        for (int i = 0; i < bookIds.Length; i++)
        {
            var r1 = reviewData[i * 2];
            var r2 = reviewData[i * 2 + 1];

            reviews.Add(new BookReview
            {
                Id = Guid.NewGuid(),
                BookId = bookIds[i],
                ReviewerName = r1.reviewer,
                Rating = r1.rating,
                ReviewText = r1.text,
                CreatedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(30, 365))
            });
            reviews.Add(new BookReview
            {
                Id = Guid.NewGuid(),
                BookId = bookIds[i],
                ReviewerName = r2.reviewer,
                Rating = r2.rating,
                ReviewText = r2.text,
                CreatedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30))
            });
        }

        return reviews;
    }
}
