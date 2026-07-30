using LibraryManagementSystem.Application.Books.DTOs;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Books.Queries.GetBookById;

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDto>
{
    private readonly IApplicationDbContext _context;

    public GetBookByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<BookDto> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Book", request.Id);

        return new BookDto(
            book.Id, book.ISBN, book.Title, book.Author, book.Publisher,
            book.PublicationYear, book.Category, book.TotalCopies, book.AvailableCopies, book.BranchId);
    }
}
