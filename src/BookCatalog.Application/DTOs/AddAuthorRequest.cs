namespace BookCatalog.Application.DTOs;

public record AddAuthorRequest(
    string FirstName,
    string LastName,
    int BirthYear,
    string Nationality,
    string? Biography = null);
