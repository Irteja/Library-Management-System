namespace LibraryManagementSystem.Application.Librarians.DTOs;

public record LibrarianDto(
    Guid Id, 
    string FirstName, 
    string LastName, 
    string Email, 
    string Phone, 
    string Username, 
    string BranchName);
