using LibraryManagementSystem.Application.Members.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Members.Queries.GetAllMembers;

public record GetAllMembersQuery : IRequest<List<MemberDto>>;
