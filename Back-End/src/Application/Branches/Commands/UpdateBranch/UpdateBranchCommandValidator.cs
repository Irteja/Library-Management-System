using FluentValidation;

namespace LibraryManagementSystem.Application.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Address).NotEmpty().MaximumLength(500);
        RuleFor(v => v.Phone).NotEmpty().MaximumLength(20);
        RuleFor(v => v.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}
