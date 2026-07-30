using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Books.Commands.UpdateBook;

public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateBookCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Book", request.Id);

        book.ISBN = request.ISBN;
        book.Title = request.Title;
        book.Author = request.Author;
        book.Publisher = request.Publisher;
        book.PublicationYear = request.PublicationYear;
        book.Category = request.Category;
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = request.AvailableCopies;

        _context.Update(book);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
