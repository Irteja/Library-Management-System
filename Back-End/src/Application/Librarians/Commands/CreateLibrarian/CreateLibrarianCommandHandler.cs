using FluentValidation.Results;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Librarians.Commands.CreateLibrarian;

public class CreateLibrarianCommandHandler : IRequestHandler<CreateLibrarianCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateLibrarianCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateLibrarianCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username, cancellationToken))
        {
            throw new ValidationException(new[] { new ValidationFailure("Username", "Username already exists.") });
        }

        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new ValidationException(new[] { new ValidationFailure("Email", "Email already exists.") });
        }

        var branchExists = await _context.Branches.AnyAsync(b => b.Id == request.BranchId, cancellationToken);
        if (!branchExists)
        {
            throw new ValidationException(new[] { new ValidationFailure("BranchId", "Branch does not exist.") });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Librarian,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Add(user);

        var librarian = new Librarian
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            UserId = user.Id,
            BranchId = request.BranchId
        };

        _context.Add(librarian);
        await _context.SaveChangesAsync(cancellationToken);

        return librarian.Id;
    }
}
