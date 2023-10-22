using System.Collections;
using Microsoft.EntityFrameworkCore;
using WorkSchedule.Applications.Common.Interfaces;

namespace WorkSchedule.Test;

public class WorkScheduleHandler
{
    private readonly IMyDb _db;
    private readonly TimeProvider _timeProvider;
    private readonly Random _random;

    public WorkScheduleHandler(TimeProvider timeProvider, IMyDb db)
    {
        _timeProvider = timeProvider;
        _db = db;
        _random = new Random();
    }

    public async Task<WorkScheduleResult> Handle(WorkScheduleCommand request)
    {
        var daysInMonth = GetMonthDays();
        await ScheduleDays(daysInMonth);

        return new WorkScheduleResult
        {
            Schedule = daysInMonth.Select(r => new DayInfo
            {
                Person = r.Person,
                Day = new DayInMonth(r.Date)
            }).ToList()
        };
    }

    private List<DayInMonth> GetMonthDays()
    {
        var now = _timeProvider.GetLocalNow();
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        return Enumerable.Range(1, daysInMonth)
            .Select(r => new DayInMonth(new DateTime(now.Year, now.Month, r)))
            .ToList();
    }

    private bool IsArrangeAllPeople(IEnumerable<DayInMonth> workDays, ICollection people)
    {
        return workDays.Count(r => string.IsNullOrWhiteSpace(r.Person) == false) > people.Count;
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
            var workMembers = workDay.Members(day);
            day.Person = IsArrangeAllPeople(daysInMonth, members)
                ? RandomFewestDaysPerson(daysInMonth, workMembers)
                : RandomPerson(workMembers);
        }
    }
}
