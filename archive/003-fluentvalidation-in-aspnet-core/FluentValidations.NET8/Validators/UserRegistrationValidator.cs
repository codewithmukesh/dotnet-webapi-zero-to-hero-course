using FluentValidation;
using FluentValidations.NET8.Requests;

namespace FluentValidations.NET8.Validators;

public class UserRegistrationValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserRegistrationValidator()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(4)
            .Must(IsValidName).WithMessage("{PropertyName} should be all letters.");

        RuleFor(x => x.LastName).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Email).EmailAddress().WithName("MailID").WithMessage("{PropertyName} is invalid! Please check!");
        RuleFor(x => x.Password).Equal(z => z.ConfirmPassword).WithMessage("Passwords do not match!");
    }

    private bool IsValidName(string name)
    {
        return name.All(Char.IsLetter);
    }
}


