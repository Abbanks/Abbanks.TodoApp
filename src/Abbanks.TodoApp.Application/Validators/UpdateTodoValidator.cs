using Abbanks.TodoApp.Application.DTOs;
using FluentValidation;

namespace Abbanks.TodoApp.Application.Validators
{
    public class UpdateTodoValidator : AbstractValidator<UpdateTodoDto>
    {
        public UpdateTodoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Invalid Todo ID")
                .Must(id => id != Guid.Empty).WithMessage("Todo ID cannot be empty");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(40).WithMessage("Title must not exceed 40 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("Due date is required");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status value");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Invalid priority value");
        }
    }
}
