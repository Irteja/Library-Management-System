using MediatR;

namespace LibraryManagementSystem.Application.Books.Commands.DeleteBook;

public record DeleteBookCommand(Guid Id) : IRequest;
