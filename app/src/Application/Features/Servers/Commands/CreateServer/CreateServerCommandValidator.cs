using FluentValidation;

namespace Application.Features.Servers.Commands.CreateServer;

public class CreateServerCommandValidator : AbstractValidator<CreateServerCommand>
{
    public CreateServerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Server name is required")
            .MaximumLength(100).WithMessage("Server name must not exceed 100 characters");

        RuleFor(x => x.HostName)
            .NotEmpty().WithMessage("Host name is required")
            .MaximumLength(255).WithMessage("Host name must not exceed 255 characters");

        RuleFor(x => x.IpAddress)
            .MaximumLength(45).WithMessage("IP address must not exceed 45 characters")
            .When(x => !string.IsNullOrEmpty(x.IpAddress));

        RuleFor(x => x.CpuWarningThreshold)
            .InclusiveBetween(0, 100).WithMessage("CPU warning threshold must be between 0 and 100");

        RuleFor(x => x.CpuCriticalThreshold)
            .InclusiveBetween(0, 100).WithMessage("CPU critical threshold must be between 0 and 100")
            .GreaterThan(x => x.CpuWarningThreshold).WithMessage("CPU critical threshold must be greater than warning threshold");

        RuleFor(x => x.MemoryWarningThreshold)
            .InclusiveBetween(0, 100).WithMessage("Memory warning threshold must be between 0 and 100");

        RuleFor(x => x.MemoryCriticalThreshold)
            .InclusiveBetween(0, 100).WithMessage("Memory critical threshold must be between 0 and 100")
            .GreaterThan(x => x.MemoryWarningThreshold).WithMessage("Memory critical threshold must be greater than warning threshold");

        RuleFor(x => x.DiskWarningThreshold)
            .InclusiveBetween(0, 100).WithMessage("Disk warning threshold must be between 0 and 100");

        RuleFor(x => x.DiskCriticalThreshold)
            .InclusiveBetween(0, 100).WithMessage("Disk critical threshold must be between 0 and 100")
            .GreaterThan(x => x.DiskWarningThreshold).WithMessage("Disk critical threshold must be greater than warning threshold");
    }
}
