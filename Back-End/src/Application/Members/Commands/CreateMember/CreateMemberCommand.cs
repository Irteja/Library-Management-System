using MediatR;

namespace LibraryManagementSystem.Application.Members.Commands.CreateMember;

public record CreateMemberCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime MembershipExpiryDate
) : IRequest<Guid>;
