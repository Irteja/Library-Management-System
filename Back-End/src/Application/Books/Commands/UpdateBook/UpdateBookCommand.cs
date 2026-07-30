using MediatR;

namespace LibraryManagementSystem.Application.Books.Commands.UpdateBook;

public record UpdateBookCommand(
    Guid Id,
    string ISBN,
    string Title,
    string Author,
    string Publisher,
    int PublicationYear,
    string Category,
    int TotalCopies,
    int AvailableCopies
) : IRequest;
