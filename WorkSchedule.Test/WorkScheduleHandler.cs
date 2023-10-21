namespace WorkSchedule.Test;

public class WorkScheduleHandler
{
    private readonly TimeProvider _timeProvider;
    private List<string> _people;

    public WorkScheduleHandler(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _people = Enumerable.Range(1, 5).Select(r => $"Person{r}").ToList();
    }

    public Task<WorkScheduleResult> Handle(WorkScheduleCommand request)
    {
        var daysInMonth = Enumerable.Range(1, DaysInMonth());

        var result = new WorkScheduleResult();
        result.Schedule = NewMethod(daysInMonth);
        return Task.FromResult(result);
    }

    private int DaysInMonth()
    {
        var localNow = _timeProvider.GetLocalNow();
        return DateTime.DaysInMonth(localNow.Year, localNow.Month);
    }

    private List<WorkDay> NewMethod(IEnumerable<int> daysInMonth)
    {
        var random = new Random();
        var workDays = new List<WorkDay>();
        foreach (var day in daysInMonth)
        {
            // find random person with shortest number of days
            if (workDays.Count > _people.Count)
            {
                var personDays = workDays
                    .GroupBy(r => r.Person)
                    .Select(r => new PersonDays(r.Key, r.Count()))
                    .ToList();
                var minDays = personDays.MinBy(r => r.Days)?.Days ?? 0;
                var minPersons = personDays.Where(r => r.Days == minDays).ToArray();
                var next = random.Next(minPersons.Length - 1);
                var person = minPersons[next];
                workDays.Add(new WorkDay() { Person = person.Name, Day = day });
            }
            else
            {
                var next = random.Next(_people.Count - 1);
                var person = _people[next];
                workDays.Add(new WorkDay() { Person = person, Day = day });
            }
        }
        return workDays;
    }

    private record PersonDays(string Name, int Days);
}
