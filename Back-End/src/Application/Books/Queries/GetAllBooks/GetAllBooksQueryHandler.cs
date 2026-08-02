using LibraryManagementSystem.Application.Books.DTOs;
using LibraryManagementSystem.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

using LibraryManagementSystem.Application.Common.Models;

namespace LibraryManagementSystem.Application.Books.Queries.GetAllBooks;

public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, PaginatedList<BookDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllBooksQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<BookDto>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Books.AsQueryable();

        if (_currentUser.Role == "Librarian" && _currentUser.UserId.HasValue)
        {
            var librarianBranchId = await _context.Librarians
                .Where(l => l.UserId == _currentUser.UserId.Value)
                .Select(l => (Guid?)l.BranchId)
                .FirstOrDefaultAsync(cancellationToken);

            if (librarianBranchId.HasValue)
            {
                query = query.Where(b => b.BranchId == librarianBranchId.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(b => b.Title.ToLower().Contains(searchTerm) || b.Author.ToLower().Contains(searchTerm));
        }

        var dtoQuery = query
            .Select(b => new BookDto(
                b.Id, b.ISBN, b.Title, b.Author, b.Publisher,
                b.PublicationYear, b.Category, b.TotalCopies, b.AvailableCopies, b.BranchId));
                
        return await PaginatedList<BookDto>.CreateAsync(dtoQuery, request.PageNumber, request.PageSize);
    }
}
