using LibraryManagementSystem.Application.Reservations.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Reservations.Queries.GetBookReservationQueue;

public record GetBookReservationQueueQuery(Guid BookId) : IRequest<List<ReservationDto>>;
