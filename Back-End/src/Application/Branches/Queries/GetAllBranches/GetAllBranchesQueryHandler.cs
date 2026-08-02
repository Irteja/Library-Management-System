using LibraryManagementSystem.Application.Branches.DTOs;
using LibraryManagementSystem.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Application.Common.Models;

namespace LibraryManagementSystem.Application.Branches.Queries.GetAllBranches;

public class GetAllBranchesQueryHandler : IRequestHandler<GetAllBranchesQuery, PaginatedList<BranchDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllBranchesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<BranchDto>> Handle(GetAllBranchesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Branches.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(b => 
                b.Name.ToLower().Contains(searchTerm) || 
                b.Address.ToLower().Contains(searchTerm));
        }

        var dtoQuery = query
            .Select(b => new BranchDto(b.Id, b.Name, b.Address, b.Phone, b.Email, b.CreatedAt, b.IsActive));
            
        return await PaginatedList<BranchDto>.CreateAsync(dtoQuery, request.PageNumber, request.PageSize);
    }
}
