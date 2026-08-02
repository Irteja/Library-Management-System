using LibraryManagementSystem.Application.Librarians.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Librarians.Queries.GetAllLibrarians;

public record GetAllLibrariansQuery : IRequest<IEnumerable<LibrarianDto>>;
