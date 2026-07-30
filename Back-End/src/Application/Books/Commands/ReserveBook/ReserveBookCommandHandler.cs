using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Books.Commands.ReserveBook;

public class ReserveBookCommandHandler : IRequestHandler<ReserveBookCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public ReserveBookCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(ReserveBookCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId && m.IsActive, cancellationToken)
            ?? throw new NotFoundException("Member", request.MemberId);

        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == request.BookId, cancellationToken)
            ?? throw new NotFoundException("Book", request.BookId);

        var existing = await _context.Reservations
            .AnyAsync(r => r.BookId == request.BookId
                        && r.MemberId == request.MemberId
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
            MemberId = request.MemberId,
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