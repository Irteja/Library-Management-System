using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Services;
using LibraryManagementSystem.Application.Reservations.DTOs;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Reservations.Queries.GetMemberReservations;

public class GetMemberReservationsQueryHandler : IRequestHandler<GetMemberReservationsQuery, List<ReservationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemberAccessService _memberAccess;

    public GetMemberReservationsQueryHandler(IApplicationDbContext context, IMemberAccessService memberAccess)
    {
        _context = context;
        _memberAccess = memberAccess;
    }

    public async Task<List<ReservationDto>> Handle(GetMemberReservationsQuery request, CancellationToken cancellationToken)
    {
        var memberId = await _memberAccess.GetAccessibleMemberIdAsync(request.MemberId, cancellationToken);

        return await _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Member)
            .Where(r => r.MemberId == memberId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .Select(r => new ReservationDto(
                r.Id,
                r.BookId,
                r.Book.Title,
                r.Book.Author,
                r.MemberId,
                r.Member.FirstName + " " + r.Member.LastName,
                r.BranchId,
                r.ReservedAt,
                r.ExpiresAt,
                r.QueuePosition,
                r.Status.ToString()
            ))
            .ToListAsync(cancellationToken);
    }
}
