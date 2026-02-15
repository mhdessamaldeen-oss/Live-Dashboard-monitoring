using FluentValidation;

namespace Application.Features.Reports.Commands.CreateReport;

public class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportCommandValidator()
    {
        RuleFor(x => x.ServerId)
            .GreaterThan(0).WithMessage("Invalid server ID");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Report title is required")
            .MaximumLength(200).WithMessage("Report title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Report description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DateRangeEnd)
            .GreaterThan(x => x.DateRangeStart)
            .When(x => x.DateRangeStart.HasValue && x.DateRangeEnd.HasValue)
            .WithMessage("End date must be after start date");
    }
}
