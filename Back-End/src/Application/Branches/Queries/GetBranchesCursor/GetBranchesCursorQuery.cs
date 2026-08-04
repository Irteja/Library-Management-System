using LibraryManagementSystem.Application.Branches.DTOs;
using LibraryManagementSystem.Application.Common.Models;
using MediatR;

namespace LibraryManagementSystem.Application.Branches.Queries.GetBranchesCursor;

public record GetBranchesCursorQuery(string? Cursor = null, int Limit = 50) : IRequest<CursorPaginatedList<BranchDto>>;
