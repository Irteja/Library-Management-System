using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Books.Commands.DeleteBook;

public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteBookCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Book", request.Id);

        _context.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
