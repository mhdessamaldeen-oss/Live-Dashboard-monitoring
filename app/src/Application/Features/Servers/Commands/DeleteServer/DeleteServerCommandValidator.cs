using FluentValidation;

namespace Application.Features.Servers.Commands.DeleteServer;

public class DeleteServerCommandValidator : AbstractValidator<DeleteServerCommand>
{
    public DeleteServerCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid server ID");
    }
}
