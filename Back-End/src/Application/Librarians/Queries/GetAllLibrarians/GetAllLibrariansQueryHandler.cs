using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Librarians.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Librarians.Queries.GetAllLibrarians;

public class GetAllLibrariansQueryHandler : IRequestHandler<GetAllLibrariansQuery, IEnumerable<LibrarianDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllLibrariansQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LibrarianDto>> Handle(GetAllLibrariansQuery request, CancellationToken cancellationToken)
    {
        var librarians = await _context.Librarians
            .Include(l => l.User)
            .Include(l => l.Branch)
            .Select(l => new LibrarianDto(
                l.Id,
                l.FirstName,
                l.LastName,
                l.Email,
                l.Phone,
                l.User.Username,
                l.Branch.Name))
            .ToListAsync(cancellationToken);

        return librarians;
    }
}
