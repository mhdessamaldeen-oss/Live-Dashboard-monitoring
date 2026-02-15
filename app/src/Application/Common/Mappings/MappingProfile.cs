using Application.DTOs.Alerts;
using Application.DTOs.Auth;
using Application.DTOs.Metrics;
using Application.DTOs.Reports;
using Application.DTOs.Servers;
using Application.Features.Servers.Commands.CreateServer;
using Application.Features.Servers.Commands.UpdateServer;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Auth / User
        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, opt => opt.Ignore());

        // Servers
        CreateMap<Server, ServerDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Location, opt => opt.MapFrom(s => s.Location))
            .ForMember(d => d.OperatingSystem, opt => opt.MapFrom(s => s.OperatingSystem));

        CreateMap<Server, ServerDetailDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Location, opt => opt.MapFrom(s => s.Location))
            .ForMember(d => d.OperatingSystem, opt => opt.MapFrom(s => s.OperatingSystem))
            .ForMember(d => d.LatestMetrics, opt => opt.Ignore()); // Handled in QueryHandler
        CreateMap<CreateServerCommand, Server>();
        CreateMap<UpdateServerCommand, Server>();

        // Metrics
        CreateMap<Metric, MetricDto>()
            .ForMember(d => d.CpuUsage, opt => opt.MapFrom(s => s.CpuUsagePercent))
            .ForMember(d => d.MemoryUsage, opt => opt.MapFrom(s => s.MemoryUsagePercent))
            .ForMember(d => d.NetworkIn, opt => opt.MapFrom(s => s.NetworkInBytesPerSec))
            .ForMember(d => d.NetworkOut, opt => opt.MapFrom(s => s.NetworkOutBytesPerSec))
            .ForMember(d => d.DiskUsage, opt => opt.MapFrom(s => s.DiskUsagePercent))
            .ForMember(d => d.DiskUsagePercent, opt => opt.MapFrom(s => s.DiskUsagePercent));

        // Alerts
        CreateMap<Alert, AlertDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Severity, opt => opt.MapFrom(s => s.Severity.ToString()));

        // Reports
        CreateMap<Report, ReportDto>();
    }
}
