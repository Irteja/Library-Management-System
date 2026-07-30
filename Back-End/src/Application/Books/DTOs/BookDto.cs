namespace LibraryManagementSystem.Application.Books.DTOs;

public record BookDto(
    Guid Id,
    string ISBN,
    string Title,
    string Author,
    string Publisher,
    int PublicationYear,
    string Category,
    int TotalCopies,
    int AvailableCopies,
    Guid BranchId
);
