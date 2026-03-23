namespace BookCatalog.Application.DTOs;

public record AddBookRequest(
    string Title,
    string ISBN,
    int PublishedYear,
    int PageCount,
    Guid AuthorId,
    string PublisherName,
    List<string> Genres);
