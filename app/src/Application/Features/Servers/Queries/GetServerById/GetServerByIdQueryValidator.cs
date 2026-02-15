using FluentValidation;

namespace Application.Features.Servers.Queries.GetServerById;

public class GetServerByIdQueryValidator : AbstractValidator<GetServerByIdQuery>
{
    public GetServerByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid server ID");
    }
}
