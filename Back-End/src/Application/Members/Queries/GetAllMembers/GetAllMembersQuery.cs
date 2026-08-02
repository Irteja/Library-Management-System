using LibraryManagementSystem.Application.Members.DTOs;
using MediatR;
using LibraryManagementSystem.Application.Common.Models;

namespace LibraryManagementSystem.Application.Members.Queries.GetAllMembers;

public record GetAllMembersQuery(string? SearchTerm = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<MemberDto>>;
