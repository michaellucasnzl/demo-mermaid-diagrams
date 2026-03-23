namespace BookCatalog.Domain.Entities;

public class Publisher
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FoundedYear { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? Website { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
