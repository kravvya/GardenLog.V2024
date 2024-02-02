using FluentValidation;
using UserManagement.Contract.Validators;

namespace GardenLogWeb.Models.UserProfile;

public record UserProfileModel : UserProfileViewModel
{
    public string Password { get; set; } = string.Empty;
    public string PasswordConfirmation { get; set; } = string.Empty;
}

public class UserProfileModelValidator : UserProfileValidator<UserProfileModel>
{
    public UserProfileModelValidator()
    {
        When(b => b.UserProfileId == null, () =>
        {
            RuleFor(p => p.Password).NotEmpty().WithMessage("Your password cannot be empty")
                 .MinimumLength(8).WithMessage("Your password length must be at least 8.")
                 .MaximumLength(16).WithMessage("Your password length must not exceed 16.")
                 .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
                 .Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter.")
                 .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.");
            // .Matches(@"[\!\?\*\.]+").WithMessage("Your password must contain at least one (!? *.).");

            RuleFor(customer => customer.Password).Equal(customer => customer.PasswordConfirmation).WithMessage("The password and confirmation password do not match");
        });
    }
}
