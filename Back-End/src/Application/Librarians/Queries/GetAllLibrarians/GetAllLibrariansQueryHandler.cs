using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Models;
using LibraryManagementSystem.Application.Librarians.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Librarians.Queries.GetAllLibrarians;

public class GetAllLibrariansQueryHandler : IRequestHandler<GetAllLibrariansQuery, PaginatedList<LibrarianDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllLibrariansQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<LibrarianDto>> Handle(GetAllLibrariansQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Librarians
            .Include(l => l.User)
            .Include(l => l.Branch)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(l => l.FirstName.Contains(request.SearchTerm) || l.LastName.Contains(request.SearchTerm) || l.User.Username.Contains(request.SearchTerm));
        }

        var projectedQuery = query.Select(l => new LibrarianDto(
                l.Id,
                l.FirstName,
                l.LastName,
                l.Email,
                l.Phone,
                l.User.Username,
                l.Branch.Name));

        return await PaginatedList<LibrarianDto>.CreateAsync(projectedQuery, request.PageNumber, request.PageSize);
    }
}
