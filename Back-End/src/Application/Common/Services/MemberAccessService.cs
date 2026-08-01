using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Common.Services;

public interface IMemberAccessService
{
    /// <summary>
    /// Resolves the member id the current user is allowed to access.
    /// Members may only access their own linked member profile; any other
    /// requested id results in a <see cref="ForbiddenAccessException"/>.
    /// </summary>
    Task<Guid> GetAccessibleMemberIdAsync(Guid requestedMemberId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the member id linked to the current user. For members this is
    /// always their own linked member profile; for staff it returns null so
    /// that staff operations are not scoped to a single member.
    /// </summary>
    Task<Guid?> GetCurrentMemberIdAsync(CancellationToken cancellationToken);
}

public class MemberAccessService : IMemberAccessService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MemberAccessService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> GetAccessibleMemberIdAsync(Guid requestedMemberId, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != "Member")
        {
            return requestedMemberId;
        }

        if (_currentUser.UserId is not { } userId)
        {
            throw new ForbiddenAccessException();
        }

        var memberId = await _context.Members
            .Where(m => m.UserId == userId)
            .Select(m => (Guid?)m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (memberId is null)
        {
            throw new ForbiddenAccessException("No member profile is linked to your account.");
        }

        if (memberId != requestedMemberId)
        {
            throw new ForbiddenAccessException("You may only access your own data.");
        }

        return memberId.Value;
    }

    /// <summary>
    /// Returns the member id linked to the current user. For members this is
    /// always their own linked member profile; for staff it returns null so
    /// that staff operations are not scoped to a single member.
    /// </summary>
    public async Task<Guid?> GetCurrentMemberIdAsync(CancellationToken cancellationToken)
    {
        if (_currentUser.Role != "Member")
        {
            return null;
        }

        if (_currentUser.UserId is not { } userId)
        {
            throw new ForbiddenAccessException();
        }

        var memberId = await _context.Members
            .Where(m => m.UserId == userId)
            .Select(m => (Guid?)m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (memberId is null)
        {
            throw new ForbiddenAccessException("No member profile is linked to your account.");
        }

        return memberId;
    }
}
