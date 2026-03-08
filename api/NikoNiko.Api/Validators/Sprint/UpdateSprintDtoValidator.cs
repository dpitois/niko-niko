using FluentValidation;

using NikoNiko.Core.DTOs.Sprint;

namespace NikoNiko.Api.Validators.Sprint;

public class UpdateSprintDtoValidator : AbstractValidator<UpdateSprintDto>
{
    public UpdateSprintDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("validation.required")
            .MaximumLength(100).WithMessage("validation.maxLength_100");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("validation.required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("validation.required")
            .GreaterThan(x => x.StartDate).WithMessage("validation.endDateBeforeStartDate");

        RuleFor(x => x)
            .Must(x => x.EndDate <= x.StartDate.AddMonths(2))
            .WithMessage("validation.sprintTooLong");
    }
}