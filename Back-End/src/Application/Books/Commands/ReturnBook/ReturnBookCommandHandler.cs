using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Books.Commands.ReturnBook;

public class ReturnBookCommandHandler : IRequestHandler<ReturnBookCommand>
{
    private readonly IApplicationDbContext _context;

    public ReturnBookCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(ReturnBookCommand request, CancellationToken cancellationToken)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken)
            ?? throw new NotFoundException("Loan", request.LoanId);

        if (loan.Status == LoanStatus.Returned)
            throw new InvalidOperationException("This book has already been returned.");

        loan.ReturnDate = DateTime.UtcNow;
        loan.Status = LoanStatus.Returned;
        loan.Book.AvailableCopies++;

        var pendingReservation = await _context.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync(cancellationToken);

        if (pendingReservation is not null)
            pendingReservation.Status = ReservationStatus.Fulfilled;

        _context.Update(loan);
        await _context.SaveChangesAsync(cancellationToken);
    }
}