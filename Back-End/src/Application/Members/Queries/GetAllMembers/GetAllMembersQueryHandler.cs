using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Members.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Application.Common.Models;

namespace LibraryManagementSystem.Application.Members.Queries.GetAllMembers;

public class GetAllMembersQueryHandler : IRequestHandler<GetAllMembersQuery, PaginatedList<MemberDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllMembersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<MemberDto>> Handle(GetAllMembersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Members.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(m => 
                m.FirstName.ToLower().Contains(searchTerm) || 
                m.LastName.ToLower().Contains(searchTerm) || 
                m.Email.ToLower().Contains(searchTerm));
        }

        var dtoQuery = query
            .Select(m => new MemberDto(
                m.Id, m.FirstName, m.LastName, m.Email, m.Phone,
                m.MembershipDate, m.MembershipExpiryDate, m.IsActive));
                
        return await PaginatedList<MemberDto>.CreateAsync(dtoQuery, request.PageNumber, request.PageSize);
    }
}
