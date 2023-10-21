using System.Collections;

namespace WorkSchedule.Test;

public class WorkScheduleHandler
{
    private readonly List<string> _people;
    private readonly TimeProvider _timeProvider;

    public WorkScheduleHandler(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _people = Enumerable.Range(1, 5).Select(r => $"Person{r}").ToList();
    }

    public Task<WorkScheduleResult> Handle(WorkScheduleCommand request)
    {
        var daysInMonth = Enumerable.Range(1, DaysInMonth());

        var result = new WorkScheduleResult();
        result.Schedule = ScheduleDays(daysInMonth);
        return Task.FromResult(result);
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

    private bool IsArrangeAllPeople(ICollection workDays)
    {
        return workDays.Count > _people.Count;
    }

    private List<WorkDay> ScheduleDays(IEnumerable<int> daysInMonth)
    {
        var random = new Random();
        var workDays = new List<WorkDay>();
        foreach (var day in daysInMonth)
        {
            var person = IsArrangeAllPeople(workDays)
                ? GetFewestDaysPerson(workDays, random)
                : RandomPerson(random);
            workDays.Add(new WorkDay { Person = person, Day = day });
        }
        return workDays;
    }

    private string RandomPerson(Random random)
    {
        var next = random.Next(_people.Count - 1);
        return _people[next];
    }

    private record PersonDays(string Name, int Days);
}
