namespace BookCatalog.Domain.Entities;

public class BookGenre
{
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;

    public Guid GenreId { get; set; }
    public Genre Genre { get; set; } = null!;
}
