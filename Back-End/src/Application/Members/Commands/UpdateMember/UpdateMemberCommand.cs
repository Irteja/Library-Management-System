using MediatR;

namespace LibraryManagementSystem.Application.Members.Commands.UpdateMember;

public record UpdateMemberCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime MembershipExpiryDate,
    bool IsActive
) : IRequest;
