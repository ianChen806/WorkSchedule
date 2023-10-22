namespace WorkSchedule.Test;

public class WorkDay
{
    private readonly List<MemberWorkDay> _members;

    public WorkDay(List<MemberWorkDay> members)
    {
        _members = members;
    }

    public List<MemberWorkDay> Members(DayInMonth day)
    {
        return _members.Where(r => r.IgnoreDays.TrueForAll(s => s != day.Date)).ToList();
    }

    public bool IsArrangeAllPeople(IEnumerable<DayInMonth> dayInMonths)
    {
        return dayInMonths
            .Count(r => string.IsNullOrWhiteSpace(r.Person) == false) > _members.Count;
    }

    public WorkDay SetIgnoreDays(List<DayInMonth> schedule)
    {
        foreach (var member in _members)
        {
            var ignoreDays = schedule.Where(r => r.Person == member.Name);
            member.IgnoreDays.AddRange(ignoreDays.Select(r => r.Date));
        }
        return this;
    }
}
