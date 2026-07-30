using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Reservations.DTOs;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Reservations.Queries.GetBookReservationQueue;

public class GetBookReservationQueueQueryHandler : IRequestHandler<GetBookReservationQueueQuery, List<ReservationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBookReservationQueueQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<ReservationDto>> Handle(GetBookReservationQueueQuery request, CancellationToken cancellationToken)
    {
        return await _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Member)
            .Where(r => r.BookId == request.BookId && r.Status == ReservationStatus.Pending)
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
