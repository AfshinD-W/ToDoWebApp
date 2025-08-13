using FluentValidation;
using SSToDo.Models.Dtos;

namespace SSToDo.Models.DtoValidator
{
    public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
    {
        public CreateProjectDtoValidator()
        {
            RuleFor(p => p.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(30).WithMessage("MaximumLenght for title is 30 character.");
        }
    }
}
