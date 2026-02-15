using FluentValidation;

namespace Application.Features.Metrics.Queries.GetMetrics;

public class GetMetricsQueryValidator : AbstractValidator<GetMetricsQuery>
{
    public GetMetricsQueryValidator()
    {
        RuleFor(x => x.ServerId)
            .GreaterThan(0).WithMessage("Invalid server ID");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 1000).WithMessage("Page size must be between 1 and 1000");

        RuleFor(x => x.To)
            .GreaterThan(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("End date must be after start date");
    }
}
