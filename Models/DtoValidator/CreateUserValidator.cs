using FluentValidation;
using SSToDo.Models.Dtos;

namespace SSToDo.Models.DtoValidator
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDtos>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(u => u.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(50);

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
                .WithMessage("Password must contain lowercase, uppercase, and a digit.");
        }
    }
}
