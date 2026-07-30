using LibraryManagementSystem.Application.Branches.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Branches.Queries.GetAllBranches;

public record GetAllBranchesQuery : IRequest<List<BranchDto>>;
