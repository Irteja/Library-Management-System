using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using MediatR;

namespace LibraryManagementSystem.Application.Books.Commands;

public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateBookCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var book = new Book
        {
            Id = Guid.NewGuid(),
            ISBN = request.ISBN,
            Title = request.Title,
            Author = request.Author,
            Publisher = request.Publisher,
            PublicationYear = request.PublicationYear,
            Category = request.Category,
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.TotalCopies,
            BranchId = request.BranchId
        };

        _context.Add(book);
        await _context.SaveChangesAsync(cancellationToken);

        return book.Id;
    }
}