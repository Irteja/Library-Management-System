using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Members.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, MemberDto>
{
    private readonly IApplicationDbContext _context;

    public GetMemberByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<MemberDto> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Member", request.Id);

        return new MemberDto(
            member.Id, member.FirstName, member.LastName, member.Email, member.Phone,
            member.MembershipDate, member.MembershipExpiryDate, member.IsActive);
    }
}
