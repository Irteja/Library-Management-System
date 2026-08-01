using LibraryManagementSystem.Application.Reservations.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Reservations.Queries.GetActiveReservations;

public record GetActiveReservationsQuery : IRequest<List<ReservationDto>>;
