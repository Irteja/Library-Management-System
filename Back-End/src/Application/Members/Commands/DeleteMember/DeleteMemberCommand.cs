using MediatR;

namespace LibraryManagementSystem.Application.Members.Commands.DeleteMember;

public record DeleteMemberCommand(Guid Id) : IRequest;
