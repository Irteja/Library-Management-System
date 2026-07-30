using LibraryManagementSystem.Application.Books.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Books.Queries.GetAllBooks;

public record GetAllBooksQuery : IRequest<List<BookDto>>;
