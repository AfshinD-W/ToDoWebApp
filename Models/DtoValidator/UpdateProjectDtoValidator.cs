using FluentValidation;
using SSToDo.Models.Dtos;

namespace SSToDo.Models.DtoValidator
{
    public class UpdateProjectDtoValidator : AbstractValidator<UpdateProjectDto>
    {
        public UpdateProjectDtoValidator()
        {
            RuleFor(p => p.Title)
                .MaximumLength(30).WithMessage("MaximumLenght for title is 30 character.");
        }
    }
}
