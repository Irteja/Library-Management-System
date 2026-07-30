namespace LibraryManagementSystem.Application.Members.DTOs;

public record MemberDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime MembershipDate,
    DateTime MembershipExpiryDate,
    bool IsActive
);
