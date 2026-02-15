using FluentValidation;

namespace Application.Features.Alerts.Commands.ResolveAlert;

public class ResolveAlertCommandValidator : AbstractValidator<ResolveAlertCommand>
{
    public ResolveAlertCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid alert ID");

        RuleFor(x => x.Resolution)
            .MaximumLength(1000).WithMessage("Resolution must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Resolution));
    }
}
