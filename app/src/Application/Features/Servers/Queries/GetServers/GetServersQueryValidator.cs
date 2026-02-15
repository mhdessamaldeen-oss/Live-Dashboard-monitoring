using FluentValidation;

namespace Application.Features.Servers.Queries.GetServers;

public class GetServersQueryValidator : AbstractValidator<GetServersQuery>
{
    public GetServersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");
    }
}
