using FluentValidation.Api.Models;

namespace FluentValidation.Api.Validators;

public class UserRegistrationValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserRegistrationValidator()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(4)
            .Must(IsValidName).WithMessage("{PropertyName} should be all letters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("{PropertyName} is not a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match.");
    }

    private static bool IsValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.All(char.IsLetter);
    }
}
