using FluentValidation;

namespace Application.Features.Reports.Queries.DownloadReport;

public class DownloadReportQueryValidator : AbstractValidator<DownloadReportQuery>
{
    public DownloadReportQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid report ID");
    }
}
