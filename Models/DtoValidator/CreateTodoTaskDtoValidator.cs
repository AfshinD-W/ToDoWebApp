using FluentValidation;
using SSToDo.Models.Dtos;

namespace SSToDo.Models.DtoValidator
{
    public class CreateTodoTaskDtoValidator : AbstractValidator<CreateTodoTaskDto>
    {
        public CreateTodoTaskDtoValidator()
        {
            RuleFor(t => t.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("MaximumLenght for title is 100 character.");

            RuleFor(t => t.Description)
                .MaximumLength(250).WithMessage("MaximumLenght for descriptiom is 250 character.");

            RuleFor(t => t.DueDate)
                .GreaterThanOrEqualTo(t => t.StartDate).WithMessage("DueDate must be greater or equal to StartDate");
        }
    }
}
