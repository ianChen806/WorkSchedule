using System.Collections;
using Microsoft.EntityFrameworkCore;
using WorkSchedule.Applications.Common.Interfaces;

namespace WorkSchedule.Test;

public class WorkScheduleHandler
{
    private readonly IMyDb _db;
    private readonly IOpenApi _openApi;
    private readonly TimeProvider _timeProvider;

    public WorkScheduleHandler(TimeProvider timeProvider, IMyDb db, IOpenApi openApi)
    {
        _timeProvider = timeProvider;
        _db = db;
        _openApi = openApi;
    }

    public async Task<WorkScheduleResult> Handle(WorkScheduleCommand request)
    {
        var members = await QueryMembers();
        var scheduleFirst = await ScheduleDays(new WorkDay(members));

        var workDay = new WorkDay(members).SetIgnoreDays(scheduleFirst);
        var scheduleSecond = await ScheduleDays(workDay);
        return new WorkScheduleResult
        {
            ScheduleFirst = scheduleFirst,
            ScheduleSecond = scheduleSecond
        };
    }

    private async Task<List<DayInMonth>> GetMonthDays()
    {
        var now = _timeProvider.GetLocalNow();
        var dayOfMonths = await _openApi.GetDays(now.Year, now.Month);
        return dayOfMonths
            .Select(r => new DayInMonth(r.Date, r.IsHoliday))
            .ToList();
    }

    private Task<List<MemberWorkDay>> QueryMembers()
    {
        return _db.Members.Include(r => r.IgnoreDays).Select(r => new MemberWorkDay
        {
            Name = r.Name,
            IgnoreDays = r.IgnoreDays.Select(s => s.Day).ToList(),
        }).ToListAsync();
    }

    private async Task<List<DayInMonth>> ScheduleDays(WorkDay workDay)
    {
        var daysInMonth = await GetMonthDays();
        foreach (var day in daysInMonth)
        {
            day.Person = workDay.GetMember(day);
        }
        return daysInMonth;
    }
}
