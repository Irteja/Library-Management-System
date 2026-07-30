namespace LibraryManagementSystem.Application.Authentication.DTOs;

public record LoginResponse(
    string Token,
    string Username,
    string Email,
    string Role
);