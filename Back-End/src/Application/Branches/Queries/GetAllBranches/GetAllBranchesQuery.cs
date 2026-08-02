using LibraryManagementSystem.Application.Branches.DTOs;
using MediatR;
using LibraryManagementSystem.Application.Common.Models;

namespace LibraryManagementSystem.Application.Branches.Queries.GetAllBranches;

public record GetAllBranchesQuery(string? SearchTerm = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<BranchDto>>;
