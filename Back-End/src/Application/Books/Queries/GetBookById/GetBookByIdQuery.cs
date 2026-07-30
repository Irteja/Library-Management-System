using LibraryManagementSystem.Application.Books.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Books.Queries.GetBookById;

public record GetBookByIdQuery(Guid Id) : IRequest<BookDto>;
