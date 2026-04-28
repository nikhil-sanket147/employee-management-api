using FluentValidation;
using NikhilTestWebApplication.Models;

namespace NikhilTestWebApplication.Validators
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserRequest>
    {
        public RegisterUserValidator() {
            RuleFor(x => x.email).NotEmpty().WithMessage("Email is required").
                EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.password).NotEmpty().WithMessage("Password is required").
                MinimumLength(6).WithMessage("Password must be atleast 6 characters");

            RuleFor(x => x.age).GreaterThan(18).WithMessage("Age must be greater than 18");
        }
    }
}
