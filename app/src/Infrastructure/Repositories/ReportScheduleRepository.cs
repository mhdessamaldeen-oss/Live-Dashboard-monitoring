using Application.Interfaces;
using Application.DTOs.Reports;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Dapper;
using System;

namespace Infrastructure.Repositories;

public class ReportScheduleRepository : Repository<ReportSchedule>, IReportScheduleRepository
{
    private readonly ApplicationDbContext _context;

    public ReportScheduleRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReportSchedule>> GetActiveSchedulesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ReportSchedules
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReportScheduleDto>> GetAllWithServerNamesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT rs.*, s.Name as ServerName 
            FROM ReportSchedules rs
            LEFT JOIN Servers s ON rs.ServerId = s.Id";
            
        var results = await _context.Connection.QueryAsync<dynamic>(sql);
        
        return results.Select(row => new ReportScheduleDto
        {
            Id = row.Id,
            Name = row.Name,
            Description = row.Description,
            CronExpression = row.CronExpression,
            LastRunAt = row.LastRunAt,
            NextRunAt = row.NextRunAt,
            IsActive = Convert.ToBoolean((object)row.IsActive),
            Recipients = row.Recipients,
            ReportType = row.ReportType,
            ServerId = row.ServerId,
            ServerName = row.ServerName ?? "Unknown Node"
        });
    }
}
