using FluentValidation;

using NikoNiko.Core.DTOs.Team;

namespace NikoNiko.Api.Validators.Team;

public class UpdateTeamDtoValidator : AbstractValidator<UpdateTeamDto>
{
    public UpdateTeamDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("validation.required")
            .MaximumLength(100).WithMessage("validation.maxLength_100");

        RuleFor(x => x.DefaultSprintDuration)
            .GreaterThan(0).WithMessage("validation.greaterThanZero")
            .When(x => x.DefaultSprintDuration.HasValue);
    }
}