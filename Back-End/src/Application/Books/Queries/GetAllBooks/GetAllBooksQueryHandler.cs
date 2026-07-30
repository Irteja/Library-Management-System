using LibraryManagementSystem.Application.Books.DTOs;
using LibraryManagementSystem.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Books.Queries.GetAllBooks;

public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, List<BookDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllBooksQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<BookDto>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        return await _context.Books
            .Select(b => new BookDto(
                b.Id, b.ISBN, b.Title, b.Author, b.Publisher,
                b.PublicationYear, b.Category, b.TotalCopies, b.AvailableCopies, b.BranchId))
            .ToListAsync(cancellationToken);
    }
}
