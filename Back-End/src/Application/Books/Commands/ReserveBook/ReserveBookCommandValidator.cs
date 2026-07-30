using FluentValidation;

namespace LibraryManagementSystem.Application.Books.Commands.ReserveBook;

public class ReserveBookCommandValidator : AbstractValidator<ReserveBookCommand>
{
    public ReserveBookCommandValidator()
    {
        RuleFor(v => v.BookId).NotEmpty();
        RuleFor(v => v.MemberId).NotEmpty();
        RuleFor(v => v.BranchId).NotEmpty();
    }
}