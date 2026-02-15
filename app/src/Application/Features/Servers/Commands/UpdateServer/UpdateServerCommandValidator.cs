using FluentValidation;

namespace Application.Features.Servers.Commands.UpdateServer;

public class UpdateServerCommandValidator : AbstractValidator<UpdateServerCommand>
{
    public UpdateServerCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid server ID");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Server name is required")
            .MaximumLength(100).WithMessage("Server name must not exceed 100 characters");

        RuleFor(x => x.HostName)
            .NotEmpty().WithMessage("Host name is required")
            .MaximumLength(255).WithMessage("Host name must not exceed 255 characters");

        RuleFor(x => x.CpuCriticalThreshold)
            .GreaterThan(x => x.CpuWarningThreshold)
            .WithMessage("CPU critical threshold must be greater than warning threshold");

        RuleFor(x => x.MemoryCriticalThreshold)
            .GreaterThan(x => x.MemoryWarningThreshold)
            .WithMessage("Memory critical threshold must be greater than warning threshold");

        RuleFor(x => x.DiskCriticalThreshold)
            .GreaterThan(x => x.DiskWarningThreshold)
            .WithMessage("Disk critical threshold must be greater than warning threshold");
    }
}
