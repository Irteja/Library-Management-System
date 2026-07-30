using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Reservations.DTOs;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Reservations.Queries.GetMemberReservations;

public class GetMemberReservationsQueryHandler : IRequestHandler<GetMemberReservationsQuery, List<ReservationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMemberReservationsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<ReservationDto>> Handle(GetMemberReservationsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Member)
            .Where(r => r.MemberId == request.MemberId && r.Status == ReservationStatus.Pending)
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
