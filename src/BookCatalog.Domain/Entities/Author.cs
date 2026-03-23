namespace BookCatalog.Domain.Entities;

public class Author
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public DateOnly BirthDate { get; set; }
    public DateOnly? DeathDate { get; set; }
    public string Nationality { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
    public string OpenLibraryKey { get; set; } = string.Empty;
    public string? Website { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
