using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Services;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Books.Commands.ReserveBook;

public class ReserveBookCommandHandler : IRequestHandler<ReserveBookCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemberAccessService _memberAccess;

    public ReserveBookCommandHandler(IApplicationDbContext context, IMemberAccessService memberAccess)
    {
        _context = context;
        _memberAccess = memberAccess;
    }

    public async Task<Guid> Handle(ReserveBookCommand request, CancellationToken cancellationToken)
    {
        var memberId = await _memberAccess.GetAccessibleMemberIdAsync(request.MemberId, cancellationToken);

        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId && m.IsActive, cancellationToken)
            ?? throw new NotFoundException("Member", memberId);

        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == request.BookId, cancellationToken)
            ?? throw new NotFoundException("Book", request.BookId);

        if (book.AvailableCopies > 0)
            throw new InvalidOperationException("Book has available copies. Please borrow instead of reserving.");

        var existing = await _context.Reservations
            .AnyAsync(r => r.BookId == request.BookId
                        && r.MemberId == memberId
                        && r.Status == ReservationStatus.Pending, cancellationToken);

        if (existing)
            throw new InvalidOperationException("You already have a pending reservation for this book.");

        var maxPosition = await _context.Reservations
            .Where(r => r.BookId == request.BookId && r.Status == ReservationStatus.Pending)
            .MaxAsync(r => (int?)r.QueuePosition, cancellationToken) ?? 0;

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            BookId = request.BookId,
            MemberId = memberId,
            BranchId = request.BranchId,
            ReservedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            QueuePosition = maxPosition + 1,
            Status = ReservationStatus.Pending
        };

        _context.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}