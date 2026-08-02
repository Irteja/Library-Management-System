using LibraryManagementSystem.Application.Common.Models;
using LibraryManagementSystem.Application.Reservations.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Reservations.Queries.GetActiveReservations;

public record GetActiveReservationsQuery(string? SearchTerm = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<ReservationDto>>;
