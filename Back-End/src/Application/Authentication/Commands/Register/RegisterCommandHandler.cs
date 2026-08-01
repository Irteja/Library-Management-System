using FluentValidation.Results;
using LibraryManagementSystem.Application.Authentication.DTOs;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Authentication.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public RegisterCommandHandler(IApplicationDbContext context, IJwtTokenGenerator tokenGenerator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var usernameExists = await _context.Users
            .AnyAsync(u => u.Username == request.Username, cancellationToken);
            
        if (usernameExists)
            throw new ValidationException(new[] { new ValidationFailure("Username", "Username already exists.") });

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);
            
        if (emailExists)
            throw new ValidationException(new[] { new ValidationFailure("Email", "Email already exists.") });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Member,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var member = new Member
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            MembershipDate = DateTime.UtcNow,
            MembershipExpiryDate = DateTime.UtcNow.AddYears(1),
            IsActive = true,
            UserId = user.Id
        };

        _context.Add(user);
        _context.Add(member);

        await _context.SaveChangesAsync(cancellationToken);

        var token = _tokenGenerator.GenerateToken(user);

        return new LoginResponse(token, user.Username, user.Email, user.Role.ToString());
    }
}
