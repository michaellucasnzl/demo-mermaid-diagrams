namespace BookCatalog.Application.DTOs;

public record AddReviewRequest(
    string ReviewerName,
    int Rating,
    string ReviewText);
