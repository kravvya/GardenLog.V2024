using FluentValidation;
using UserManagement.Contract.Base;

namespace UserManagement.Contract.Validators;

public class UserProfileValidator<T> :AbstractValidator<T>
    where T : UserProfileBase
{
    public UserProfileValidator()
    {
        RuleFor(command => command.UserName).NotEmpty().Length(3, 50);
        RuleFor(command => command.EmailAddress).NotEmpty().EmailAddress();
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(50);
    }
}
