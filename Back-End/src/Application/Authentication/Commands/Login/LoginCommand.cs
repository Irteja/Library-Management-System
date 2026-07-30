using LibraryManagementSystem.Application.Authentication.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Authentication.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;