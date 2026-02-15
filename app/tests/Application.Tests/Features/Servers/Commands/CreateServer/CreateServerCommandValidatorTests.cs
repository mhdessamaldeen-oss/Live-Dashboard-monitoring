using Application.Features.Servers.Commands.CreateServer;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.Tests.Features.Servers.Commands.CreateServer;

public class CreateServerCommandValidatorTests
{
    private readonly CreateServerCommandValidator _validator;

    public CreateServerCommandValidatorTests()
    {
        _validator = new CreateServerCommandValidator();
    }

    [Fact]
    public void Should_HaveError_When_NameIsTooLong()
    {
        var command = new CreateServerCommand(new string('A', 101), "Host", "1.1.1.1", null, null, null);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_When_ThresholdsAreInvalid()
    {
        var command = new CreateServerCommand(
            Name: "Server",
            HostName: "Host",
            IpAddress: "1.1.1.1",
            Description: null,
            Location: null,
            OperatingSystem: null,
            CpuWarningThreshold: 90,
            CpuCriticalThreshold: 80 // Critical < Warning
        );
        
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CpuCriticalThreshold);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Should_HaveError_When_ThresholdIsOutOfRange(double threshold)
    {
        var command = new CreateServerCommand(
            Name: "Server",
            HostName: "Host",
            IpAddress: "1.1.1.1",
            Description: null,
            Location: null,
            OperatingSystem: null,
            CpuWarningThreshold: threshold
        );

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CpuWarningThreshold);
    }
}
