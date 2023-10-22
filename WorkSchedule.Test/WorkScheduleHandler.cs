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
        var daysInMonth = GetMonthDays();
        await ScheduleDays(daysInMonth);

        return new WorkScheduleResult
        {
            Schedule = daysInMonth
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

    private bool IsArrangeAllPeople(ICollection people, IEnumerable<DayInMonth> dayInMonths)
    {
        return dayInMonths
            .Count(r => string.IsNullOrWhiteSpace(r.Person) == false) > people.Count;
    }

    private Task<List<MemberWorkDay>> QueryMembers()
    {
        return _db.Members.Include(r => r.IgnoreDays).Select(r => new MemberWorkDay
        {
            Name = r.Name,
            IgnoreDays = r.IgnoreDays.Select(s => s.Day).ToList(),
        }).ToListAsync();
    }

    private string RandomFewestDaysPerson(IEnumerable<DayInMonth> workDays, IReadOnlyCollection<MemberWorkDay> workMembers)
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

    private async Task ScheduleDays(List<DayInMonth> daysInMonth)
    {
        var members = await QueryMembers();
        var workDay = new WorkDay(members);
        foreach (var day in daysInMonth)
        {
            var dayInMonths = daysInMonth.Where(r => r.IsHoliday == day.IsHoliday).ToList();
            var workMembers = workDay.Members(day);
            day.Person = IsArrangeAllPeople(members, dayInMonths)
                ? RandomFewestDaysPerson(dayInMonths, workMembers)
                : RandomPerson(workMembers);
        }
    }
}
