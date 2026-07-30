namespace LibraryManagementSystem.Application.Branches.DTOs;

public record BranchDto(
    Guid Id,
    string Name,
    string Address,
    string Phone,
    string Email,
    DateTime CreatedAt,
    bool IsActive
);
