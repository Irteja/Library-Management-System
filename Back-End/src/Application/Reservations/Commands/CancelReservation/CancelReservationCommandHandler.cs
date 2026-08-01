using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Services;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Reservations.Commands.CancelReservation;

public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemberAccessService _memberAccess;

    public CancelReservationCommandHandler(IApplicationDbContext context, IMemberAccessService memberAccess)
    {
        _context = context;
        _memberAccess = memberAccess;
    }

    public async Task Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Reservation", request.Id);

        if (reservation.Status != ReservationStatus.Pending)
            throw new InvalidOperationException("Only pending reservations can be cancelled.");

        var currentMemberId = await _memberAccess.GetCurrentMemberIdAsync(cancellationToken);
        if (currentMemberId is not null && reservation.MemberId != currentMemberId)
        {
            throw new ForbiddenAccessException("You may only cancel your own reservations.");
        }

        reservation.Status = ReservationStatus.Cancelled;

        _context.Update(reservation);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
