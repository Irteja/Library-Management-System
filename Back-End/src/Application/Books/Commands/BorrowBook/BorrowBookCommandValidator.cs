using FluentValidation;

namespace LibraryManagementSystem.Application.Books.Commands.BorrowBook;

public class BorrowBookCommandValidator : AbstractValidator<BorrowBookCommand>
{
    public BorrowBookCommandValidator()
    {
        RuleFor(v => v.BookId).NotEmpty();
        RuleFor(v => v.MemberId).NotEmpty();
        RuleFor(v => v.BranchId).NotEmpty();
    }
}