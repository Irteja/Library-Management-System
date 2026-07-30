using MediatR;

namespace LibraryManagementSystem.Application.Books.Commands;

public record CreateBookCommand(
    string ISBN,
    string Title,
    string Author,
    string Publisher,
    int PublicationYear,
    string Category,
    int TotalCopies,
    Guid BranchId
) : IRequest<Guid>;