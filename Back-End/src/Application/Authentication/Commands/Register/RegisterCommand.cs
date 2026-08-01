using LibraryManagementSystem.Application.Authentication.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Authentication.Commands.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Phone
) : IRequest<LoginResponse>;
