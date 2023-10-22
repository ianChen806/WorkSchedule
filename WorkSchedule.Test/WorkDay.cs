namespace WorkSchedule.Test;

public class WorkDay
{
    private readonly Dictionary<string, DayCount> _memberCounts;
    private readonly List<MemberWorkDay> _members;

    public WorkDay(List<MemberWorkDay> members)
    {
        _members = members;
        _memberCounts = members.ToDictionary(r => r.Name, r => new DayCount());
    }

    public string RandomMember(DayInMonth day)
    {
        var availableMembers = _members.Where(r => r.IgnoreDays.TrueForAll(s => s != day.Date)).Select(r => r.Name);
        var memberCounts = _memberCounts.Where(r => availableMembers.Contains(r.Key)).Select(r => new
        {
            r.Key,
            Value = day.IsHoliday ? r.Value.Holidays : r.Value.WorkDays
        }).ToList();
        var minValue = memberCounts.Min(r => r.Value);
        var minValueMembers = memberCounts.Where(r => r.Value == minValue).ToArray();

        var random = new Random(Guid.NewGuid().GetHashCode());
        var next = random.Next(minValueMembers.Length - 1);
        var person = minValueMembers[next].Key;
        AddMemberDays(person, day);
        return person;
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

    private void AddMemberDays(string person, DayInMonth day)
    {
        var memberCount = _memberCounts[person];
        if (day.IsHoliday)
        {
            memberCount.Holidays += 1;
        }
        else
        {
            memberCount.WorkDays += 1;
        }
    }
}
