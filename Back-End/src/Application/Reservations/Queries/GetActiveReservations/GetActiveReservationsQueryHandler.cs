using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Models;
using LibraryManagementSystem.Application.Reservations.DTOs;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Reservations.Queries.GetActiveReservations;

public class GetActiveReservationsQueryHandler : IRequestHandler<GetActiveReservationsQuery, PaginatedList<ReservationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveReservationsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<ReservationDto>> Handle(GetActiveReservationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Member)
            .Where(r => r.Status == ReservationStatus.Pending)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(r => r.Book.Title.Contains(request.SearchTerm) || r.Member.FirstName.Contains(request.SearchTerm) || r.Member.LastName.Contains(request.SearchTerm));
        }

        var projectedQuery = query.OrderBy(r => r.ReservedAt)
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
            ));

        return await PaginatedList<ReservationDto>.CreateAsync(projectedQuery, request.PageNumber, request.PageSize);
    }
}
