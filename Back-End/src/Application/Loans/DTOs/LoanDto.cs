namespace LibraryManagementSystem.Application.Loans.DTOs;

public record LoanDto(
    Guid Id,
    Guid BookId,
    string BookTitle,
    string BookAuthor,
    Guid MemberId,
    string MemberName,
    Guid BranchId,
    DateTime LoanDate,
    DateTime DueDate,
    DateTime? ReturnDate,
    string Status
);
