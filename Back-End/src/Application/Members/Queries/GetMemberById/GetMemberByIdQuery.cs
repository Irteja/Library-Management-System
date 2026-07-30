using LibraryManagementSystem.Application.Members.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Members.Queries.GetMemberById;

public record GetMemberByIdQuery(Guid Id) : IRequest<MemberDto>;
