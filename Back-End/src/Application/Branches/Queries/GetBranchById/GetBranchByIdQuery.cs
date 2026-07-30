using LibraryManagementSystem.Application.Branches.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Branches.Queries.GetBranchById;

public record GetBranchByIdQuery(Guid Id) : IRequest<BranchDto>;
