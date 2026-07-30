using FluentValidation;

namespace LibraryManagementSystem.Application.Branches.Commands.CreateBranch;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Address).NotEmpty().MaximumLength(500);
        RuleFor(v => v.Phone).NotEmpty().MaximumLength(20);
        RuleFor(v => v.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}
