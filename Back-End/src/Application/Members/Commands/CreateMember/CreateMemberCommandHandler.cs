using FluentValidation.Results;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Members.Commands.CreateMember;

public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateMemberCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username, cancellationToken))
        {
            throw new ValidationException(new[] { new ValidationFailure("Username", "Username already exists.") });
        }

        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new ValidationException(new[] { new ValidationFailure("Email", "Email already exists.") });
        }

        if (!Enum.TryParse<UserRole>(request.Role, out var role))
        {
            role = UserRole.Member;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Add(user);

        var member = new Member
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            MembershipDate = DateTime.UtcNow,
            MembershipExpiryDate = request.MembershipExpiryDate,
            IsActive = true,
            UserId = user.Id
        };

        _context.Add(member);
        await _context.SaveChangesAsync(cancellationToken);

        return member.Id;
    }
}
