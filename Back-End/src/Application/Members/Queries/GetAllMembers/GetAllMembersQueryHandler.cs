using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Members.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Members.Queries.GetAllMembers;

public class GetAllMembersQueryHandler : IRequestHandler<GetAllMembersQuery, List<MemberDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllMembersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<MemberDto>> Handle(GetAllMembersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Members
            .Select(m => new MemberDto(
                m.Id, m.FirstName, m.LastName, m.Email, m.Phone,
                m.MembershipDate, m.MembershipExpiryDate, m.IsActive))
            .ToListAsync(cancellationToken);
    }
}
