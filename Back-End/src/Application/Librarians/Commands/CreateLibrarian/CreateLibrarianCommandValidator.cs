using FluentValidation;

namespace LibraryManagementSystem.Application.Librarians.Commands.CreateLibrarian;

public class CreateLibrarianCommandValidator : AbstractValidator<CreateLibrarianCommand>
{
    public CreateLibrarianCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.LastName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(v => v.Phone).NotEmpty().MaximumLength(20);
        RuleFor(v => v.Username).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Password).NotEmpty().MinimumLength(6);
        RuleFor(v => v.BranchId).NotEmpty();
    }
}
