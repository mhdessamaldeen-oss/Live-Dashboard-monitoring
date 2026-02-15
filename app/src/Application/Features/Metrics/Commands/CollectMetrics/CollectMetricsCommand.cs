using Application.Common;
using MediatR;

namespace Application.Features.Metrics.Commands.CollectMetrics;

/// <summary>
/// Command used by Hangfire recurring job to collect metrics for a server
/// </summary>
public record CollectMetricsCommand(int ServerId) : IRequest<Result>;
