using FluentValidation;

namespace LibraryManagementSystem.Application.Members.Commands.CreateMember;

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.LastName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(v => v.Phone).NotEmpty().MaximumLength(20);
        RuleFor(v => v.MembershipExpiryDate).GreaterThan(DateTime.UtcNow);
    }
}
