using FluentValidation;

namespace LibraryManagementSystem.Application.Books.Commands.ReturnBook;

public class ReturnBookCommandValidator : AbstractValidator<ReturnBookCommand>
{
    public ReturnBookCommandValidator()
    {
        RuleFor(v => v.LoanId).NotEmpty();
    }
}