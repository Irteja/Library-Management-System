using LibraryManagementSystem.Application.Reservations.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Reservations.Queries.GetMemberReservations;

public record GetMemberReservationsQuery(Guid MemberId) : IRequest<List<ReservationDto>>;
