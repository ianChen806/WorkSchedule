using System.Collections;
using Microsoft.EntityFrameworkCore;
using WorkSchedule.Applications.Common.Interfaces;

namespace WorkSchedule.Test;

public class WorkScheduleHandler
{
    private readonly IMyDb _db;
    private readonly IOpenApi _openApi;
    private readonly Random _random;
    private readonly TimeProvider _timeProvider;

    public WorkScheduleHandler(TimeProvider timeProvider, IMyDb db, IOpenApi openApi)
    {
        _timeProvider = timeProvider;
        _db = db;
        _openApi = openApi;
        _random = new Random();
    }

    public async Task<WorkScheduleResult> Handle(WorkScheduleCommand request)
    {
        var members = await QueryMembers();
        var scheduleFirst = ScheduleDays(new WorkDay(members));

        var workDay = new WorkDay(members).SetIgnoreDays(scheduleFirst);
        var scheduleSecond = ScheduleDays(workDay);
        return new WorkScheduleResult
        {
            ScheduleFirst = scheduleFirst,
            ScheduleSecond = scheduleSecond
        };
    }

    private List<DayInMonth> GetMonthDays()
    {
        var now = _timeProvider.GetLocalNow();
        var dayOfMonths = _openApi.GetDays(now.Year, now.Month);
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

    private string RandomFewestDaysPerson(IReadOnlyCollection<MemberWorkDay> workMembers, IEnumerable<DayInMonth> workDays)
    {
        var personDays = workDays
            .Where(r => workMembers.Any(s => s.Name == r.Person))
            .GroupBy(r => r.Person)
            .Select(r => new PersonDays(r.Key, r.Count()))
            .ToList();
        var minDays = personDays.MinBy(r => r.Days)?.Days ?? 0;
        var minPersons = personDays.Where(r => r.Days == minDays).ToArray();
        var next = _random.Next(minPersons.Length - 1);
        return minPersons[next].Name;
    }

    private string RandomPerson(IReadOnlyList<MemberWorkDay> members)
    {
        var next = _random.Next(members.Count - 1);
        return members[next].Name;
    }

    private List<DayInMonth> ScheduleDays(WorkDay workDay)
    {
        var daysInMonth = GetMonthDays();
        foreach (var day in daysInMonth)
        {
            var dayInMonths = daysInMonth.Where(r => r.IsHoliday == day.IsHoliday).ToList();
            var workMembers = workDay.Members(day);
            day.Person = workDay.IsArrangeAllPeople(dayInMonths)
                ? RandomFewestDaysPerson(workMembers, dayInMonths)
                : RandomPerson(workMembers);
        }
        return daysInMonth;
    }
}
