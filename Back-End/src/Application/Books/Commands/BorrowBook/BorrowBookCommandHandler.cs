using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Services;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Books.Commands.BorrowBook;

public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemberAccessService _memberAccess;

    public BorrowBookCommandHandler(IApplicationDbContext context, IMemberAccessService memberAccess)
    {
        _context = context;
        _memberAccess = memberAccess;
    }

    public async Task<Guid> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        var memberId = await _memberAccess.GetAccessibleMemberIdAsync(request.MemberId, cancellationToken);

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId && m.IsActive, cancellationToken)
            ?? throw new NotFoundException("Member", memberId);

        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == request.BookId, cancellationToken)
            ?? throw new NotFoundException("Book", request.BookId);

        if (book.BranchId != request.BranchId)
            throw new InvalidOperationException("The requested book is not available in the selected branch.");

        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException("No copies available. Please place a reservation instead.");

        var activeLoansCount = await _context.Loans
            .CountAsync(l => l.MemberId == memberId
                          && l.Status == LoanStatus.Active, cancellationToken);

        if (activeLoansCount >= 5)
            throw new InvalidOperationException("Member has reached the maximum borrowing limit of 5 active loans.");

        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            BookId = request.BookId,
            MemberId = memberId,
            BranchId = request.BranchId,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            Status = LoanStatus.Active
        };

        book.AvailableCopies--;

        _context.Add(loan);
        _context.Update(book);
        await _context.SaveChangesAsync(cancellationToken);

        return loan.Id;
    }
}