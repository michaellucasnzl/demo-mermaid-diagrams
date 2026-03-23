namespace BookCatalog.Domain.Entities;

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublishedYear { get; set; }
    public string Description { get; set; } = string.Empty;
    public int PageCount { get; set; }
    public string? CoverUrl { get; set; }
    public string OpenLibraryKey { get; set; } = string.Empty;

    public Guid AuthorId { get; set; }
    public Author Author { get; set; } = null!;

    public Guid PublisherId { get; set; }
    public Publisher Publisher { get; set; } = null!;

    public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
    public ICollection<BookReview> Reviews { get; set; } = new List<BookReview>();
}
