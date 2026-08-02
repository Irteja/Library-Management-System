using LibraryManagementSystem.Application.Common.Models;
using LibraryManagementSystem.Application.Librarians.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Librarians.Queries.GetAllLibrarians;

public record GetAllLibrariansQuery(string? SearchTerm = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<LibrarianDto>>;
