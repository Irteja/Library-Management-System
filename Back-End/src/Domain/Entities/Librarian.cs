namespace LibraryManagementSystem.Domain.Entities;

public class Librarian
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid BranchId { get; set; }

    public User User { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
