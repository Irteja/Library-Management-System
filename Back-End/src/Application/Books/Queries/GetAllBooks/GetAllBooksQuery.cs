using LibraryManagementSystem.Application.Books.DTOs;
using MediatR;
using LibraryManagementSystem.Application.Common.Models;

namespace LibraryManagementSystem.Application.Books.Queries.GetAllBooks;

public record GetAllBooksQuery(string? SearchTerm = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<BookDto>>;
