using System.Collections;
using Microsoft.EntityFrameworkCore;
using WorkSchedule.Applications.Common.Interfaces;

namespace WorkSchedule.Test;

public class WorkScheduleHandler
{
    private readonly TimeProvider _timeProvider;
    private readonly IMyDb _db;

    public WorkScheduleHandler(TimeProvider timeProvider, IMyDb db)
    {
        _timeProvider = timeProvider;
        _db = db;
    }

    public async Task<WorkScheduleResult> Handle(WorkScheduleCommand request)
    {
        var daysInMonth = Enumerable.Range(1, DaysInMonth());

        var result = new WorkScheduleResult();
        result.Schedule = await ScheduleDays(daysInMonth);
        return result;
    }

    private int DaysInMonth()
    {
        var localNow = _timeProvider.GetLocalNow();
        return DateTime.DaysInMonth(localNow.Year, localNow.Month);
    }

    private string GetFewestDaysPerson(List<WorkDay> workDays, Random random)
    {
        var personDays = workDays
            .GroupBy(r => r.Person)
            .Select(r => new PersonDays(r.Key, r.Count()))
            .ToList();
        var minDays = personDays.MinBy(r => r.Days)?.Days ?? 0;
        var minPersons = personDays.Where(r => r.Days == minDays).ToArray();
        var next = random.Next(minPersons.Length - 1);
        var person = minPersons[next];
        return person.Name;
    }

    private bool IsArrangeAllPeople(ICollection workDays, List<string> people)
    {
        return workDays.Count > people.Count;
    }

    private async Task<List<WorkDay>> ScheduleDays(IEnumerable<int> daysInMonth)
    {
        var members = await _db.Members.Select(r => r.Name).ToListAsync();
        var random = new Random();
        var workDays = new List<WorkDay>();
        foreach (var day in daysInMonth)
        {
            var person = IsArrangeAllPeople(workDays, members)
                ? GetFewestDaysPerson(workDays, random)
                : RandomPerson(random, members);
            workDays.Add(new WorkDay { Person = person, Day = day });
        }
        return workDays;
    }

    private string RandomPerson(Random random, List<string> members)
    {
        var people = members;
        var next = random.Next(people.Count - 1);
        return people[next];
    }

    private record PersonDays(string Name, int Days);
}
