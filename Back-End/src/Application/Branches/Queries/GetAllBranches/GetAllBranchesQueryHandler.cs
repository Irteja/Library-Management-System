using LibraryManagementSystem.Application.Branches.DTOs;
using LibraryManagementSystem.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Branches.Queries.GetAllBranches;

public class GetAllBranchesQueryHandler : IRequestHandler<GetAllBranchesQuery, List<BranchDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllBranchesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<BranchDto>> Handle(GetAllBranchesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Branches
            .Select(b => new BranchDto(b.Id, b.Name, b.Address, b.Phone, b.Email, b.CreatedAt, b.IsActive))
            .ToListAsync(cancellationToken);
    }
}
