using MediatR;

namespace LibraryManagementSystem.Application.Books.Commands.ReturnBook;

public record ReturnBookCommand(Guid LoanId) : IRequest;