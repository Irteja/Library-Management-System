using MediatR;

namespace LibraryManagementSystem.Application.Reservations.Commands.CancelReservation;

public record CancelReservationCommand(Guid Id) : IRequest;
