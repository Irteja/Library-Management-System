using FluentValidation;

namespace LibraryManagementSystem.Application.Books.Commands.UpdateBook;

public class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.ISBN).NotEmpty().MaximumLength(20);
        RuleFor(v => v.Title).NotEmpty().MaximumLength(300);
        RuleFor(v => v.Author).NotEmpty().MaximumLength(200);
        RuleFor(v => v.PublicationYear)
            .InclusiveBetween(1000, DateTime.UtcNow.Year);
        RuleFor(v => v.TotalCopies).GreaterThan(0);
        RuleFor(v => v.AvailableCopies).GreaterThanOrEqualTo(0);
    }
}
