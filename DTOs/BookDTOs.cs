namespace LibraryManagementSystem.DTOs;

public record CreateBookRequest(
    string Title, string Author, string ISBN,
    string Category, string Publisher, int PublishedYear, int TotalCopies);

public record UpdateBookRequest(
    string Title, string Author, string Category,
    string Publisher, int PublishedYear, int TotalCopies);

public record BookResponse(
    int Id, string Title, string Author, string ISBN,
    string Category, string Publisher, int PublishedYear,
    int TotalCopies, int AvailableCopies);
