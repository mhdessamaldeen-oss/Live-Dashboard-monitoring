using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Authorize]
[Route("api/v1/settings")]
public class SettingsController : ApiControllerBase
{
    private readonly IMetricsProviderSettings _settings;

    public SettingsController(IMetricsProviderSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Get the current metrics provider mode and OS info.
    /// </summary>
    [HttpGet("metrics-provider")]
    public ActionResult<MetricsProviderResponse> GetMetricsProvider()
    {
        return Ok(BuildResponse());
    }

    /// <summary>
    /// Set the metrics provider mode (Mock or System).
    /// System mode is supported on Windows and Linux.
    /// </summary>
    [HttpPost("metrics-provider")]
    public ActionResult<MetricsProviderResponse> SetMetricsProvider([FromBody] SetMetricsProviderRequest request)
    {
        if (!Enum.TryParse<MetricsProviderMode>(request.Mode, true, out var mode))
        {
            return BadRequest($"Invalid mode '{request.Mode}'. Use 'Mock' or 'System'.");
        }

        if (mode == MetricsProviderMode.System && !IsSystemSupported())
        {
            return BadRequest("System metrics provider is only supported on Windows and Linux. This host OS is not supported.");
        }

        _settings.CurrentMode = mode;

        return Ok(BuildResponse());
    }

    private static bool IsSystemSupported() => OperatingSystem.IsWindows() || OperatingSystem.IsLinux();

    private MetricsProviderResponse BuildResponse() => new()
    {
        Mode = _settings.CurrentMode.ToString(),
        IsSystemSupported = IsSystemSupported(),
        OsPlatform = OperatingSystem.IsWindows() ? "Windows" :
                     OperatingSystem.IsLinux() ? "Linux" : "Unsupported"
    };
}

public class SetMetricsProviderRequest
{
    public string Mode { get; set; } = string.Empty;
}

public class MetricsProviderResponse
{
    public string Mode { get; set; } = string.Empty;
    public bool IsSystemSupported { get; set; }
    public string OsPlatform { get; set; } = string.Empty;
}
