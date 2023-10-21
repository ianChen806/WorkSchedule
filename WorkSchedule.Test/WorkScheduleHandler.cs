using System.Collections;
using Microsoft.EntityFrameworkCore;
using WorkSchedule.Applications.Common.Interfaces;

namespace WorkSchedule.Test;

public class WorkScheduleHandler
{
    private readonly IMyDb _db;
    private readonly TimeProvider _timeProvider;

    public WorkScheduleHandler(TimeProvider timeProvider, IMyDb db)
    {
        _timeProvider = timeProvider;
        _db = db;
    }

    public async Task<WorkScheduleResult> Handle(WorkScheduleCommand request)
    {
        var daysInMonth = GetMonthDays();

        var result = new WorkScheduleResult();
        result.Schedule = await ScheduleDays(daysInMonth);
        return result;
    }

    private IEnumerable<DateTime> GetMonthDays()
    {
        var now = _timeProvider.GetLocalNow();
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        return Enumerable.Range(1, daysInMonth).Select(r => new DateTime(now.Year, now.Month, r));
    }

    private string RandomPerson(Random random, List<MemberWorkDay> members)
    {
        var next = random.Next(members.Count - 1);
        return members[next].Name;
    }

    private async Task<List<DayInfo>> ScheduleDays(IEnumerable<DateTime> daysInMonth)
    {
        var members = await QueryMembers();
        var workDay = new WorkDay(members);
        var random = new Random();
        var workDays = new List<DayInfo>();
        foreach (var day in daysInMonth)
        {
            var workMembers = workDay.Members(day);
            var person = IsArrangeAllPeople(workDays, members)
                ? GetFewestDaysPerson(workDays, random, workMembers)
                : RandomPerson(random, workMembers);
            workDays.Add(new DayInfo { Person = person, Day = day });
        }
        return workDays;
    }

    private Task<List<MemberWorkDay>> QueryMembers()
    {
        return _db.Members.Include(r => r.IgnoreDays).Select(r => new MemberWorkDay
        {
            Name = r.Name,
            IgnoreDays = r.IgnoreDays.Select(s => s.Day).ToList(),
        }).ToListAsync();
    }

    private string GetFewestDaysPerson(List<DayInfo> workDays, Random random, List<MemberWorkDay> workMembers)
    {
        var personDays = workDays
            .Where(r => workMembers.Any(s => s.Name == r.Person))
            .GroupBy(r => r.Person)
            .Select(r => new PersonDays(r.Key, r.Count()))
            .ToList();
        var minDays = personDays.MinBy(r => r.Days)?.Days ?? 0;
        var minPersons = personDays.Where(r => r.Days == minDays).ToArray();
        var next = random.Next(minPersons.Length - 1);
        return minPersons[next].Name;
    }

    private bool IsArrangeAllPeople(ICollection workDays, List<MemberWorkDay> people)
    {
        return workDays.Count > people.Count;
    }
}
