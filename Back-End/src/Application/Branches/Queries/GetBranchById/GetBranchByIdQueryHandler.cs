using LibraryManagementSystem.Application.Branches.DTOs;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Branches.Queries.GetBranchById;

public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, BranchDto>
{
    private readonly IApplicationDbContext _context;

    public GetBranchByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<BranchDto> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Branch", request.Id);

        return new BranchDto(branch.Id, branch.Name, branch.Address, branch.Phone, branch.Email, branch.CreatedAt, branch.IsActive);
    }
}
