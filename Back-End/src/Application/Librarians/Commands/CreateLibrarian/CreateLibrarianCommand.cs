using MediatR;

namespace LibraryManagementSystem.Application.Librarians.Commands.CreateLibrarian;

public record CreateLibrarianCommand : IRequest<Guid>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public Guid BranchId { get; init; }
}
