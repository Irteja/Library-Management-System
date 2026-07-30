using FluentValidation;

namespace LibraryManagementSystem.Application.Books.Commands;

public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(v => v.ISBN)
            .NotEmpty().WithMessage("ISBN is required.")
            .MaximumLength(20);

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(300);

        RuleFor(v => v.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(200);

        RuleFor(v => v.PublicationYear)
            .InclusiveBetween(1000, DateTime.UtcNow.Year).WithMessage("Publication year must be between 1000 and the current year.");

        RuleFor(v => v.TotalCopies)
            .GreaterThan(0).WithMessage("Total copies must be greater than zero.");

        RuleFor(v => v.BranchId)
            .NotEmpty().WithMessage("BranchId is required.");
    }
}