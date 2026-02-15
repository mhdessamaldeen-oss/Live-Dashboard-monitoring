using Application.DTOs.Reports;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NCrontab;

namespace WebAPI.Controllers;

[Authorize]
public class ReportSchedulesController : ApiControllerBase
{
    private readonly IReportScheduleRepository _scheduleRepository;
    private readonly ICurrentUserService _currentUserService;

    public ReportSchedulesController(
        IReportScheduleRepository scheduleRepository, 
        ICurrentUserService currentUserService)
    {
        _scheduleRepository = scheduleRepository;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportScheduleDto>>> GetSchedules()
    {
        var schedules = await _scheduleRepository.GetAllWithServerNamesAsync();
        return Ok(schedules);
    }

    [HttpPost]
    public async Task<ActionResult<ReportScheduleDto>> CreateSchedule(CreateReportScheduleRequest request)
    {
        try
        {
            CrontabSchedule.Parse(request.CronExpression);
        }
        catch
        {
            return BadRequest("Invalid Cron Expression");
        }

        var schedule = new ReportSchedule
        {
            Name = request.Name,
            Description = request.Description,
            CronExpression = request.CronExpression,
            Recipients = request.Recipients,
            ReportType = request.ReportType,
            ServerId = request.ServerId,
            CreatedByUserId = _currentUserService.UserId ?? 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var crontab = CrontabSchedule.Parse(schedule.CronExpression);
        schedule.NextRunAt = crontab.GetNextOccurrence(DateTime.UtcNow);

        await _scheduleRepository.AddAsync(schedule);
        await _scheduleRepository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSchedules), new { id = schedule.Id }, schedule);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSchedule(int id, UpdateReportScheduleRequest request)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null) return NotFound();

        schedule.Name = request.Name;
        schedule.Description = request.Description;
        schedule.CronExpression = request.CronExpression;
        schedule.Recipients = request.Recipients;
        schedule.ReportType = request.ReportType;
        schedule.ServerId = request.ServerId;
        schedule.IsActive = request.IsActive;
        schedule.UpdatedAt = DateTime.UtcNow;

        try
        {
            var crontab = CrontabSchedule.Parse(schedule.CronExpression);
            schedule.NextRunAt = crontab.GetNextOccurrence(DateTime.UtcNow);
        }
        catch
        {
            return BadRequest("Invalid Cron Expression");
        }

        _scheduleRepository.Update(schedule);
        await _scheduleRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/toggle")]
    public async Task<IActionResult> ToggleSchedule(int id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null) return NotFound();

        schedule.IsActive = !schedule.IsActive;
        
        if (schedule.IsActive)
        {
            var crontab = CrontabSchedule.Parse(schedule.CronExpression);
            schedule.NextRunAt = crontab.GetNextOccurrence(DateTime.UtcNow);
        }
        else
        {
            schedule.NextRunAt = null;
        }

        await _scheduleRepository.SaveChangesAsync();
        return Ok(new { isActive = schedule.IsActive });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null) return NotFound();

        _scheduleRepository.Remove(schedule);
        await _scheduleRepository.SaveChangesAsync();

        return NoContent();
    }
}
