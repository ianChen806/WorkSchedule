namespace WorkSchedule.Applications.WorkSchedule;

public class WorkDay
{
    private readonly Dictionary<string, DayCount> _memberCounts;
    private readonly List<MemberWorkDay> _members;
    private readonly Random _random;

    public WorkDay(List<MemberWorkDay> members)
    {
        _members = members;
        _memberCounts = members.ToDictionary(r => r.Name, r => new DayCount());
        _random = new Random(Guid.NewGuid().GetHashCode());
    }

    public string GetMember(DayInMonth day)
    {
        var availableMembers = AvailableMembers(day);
        var leastDaysMembers = LeastDaysMember(day, availableMembers);
        var member = RandomMember(leastDaysMembers);
        IncreaseMemberDays(day.IsHoliday, member);
        return member;
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

    private IEnumerable<string> AvailableMembers(DayInMonth day)
    {
        return _members.Where(r => r.IgnoreDays.TrueForAll(s => s != day.Date)).Select(r => r.Name);
    }

    private void IncreaseMemberDays(bool isHoliday, string person)
    {
        var memberCount = _memberCounts[person];
        if (isHoliday)
        {
            memberCount.Holidays += 1;
        }
        else
        {
            memberCount.WorkDays += 1;
        }
    }

    private string[] LeastDaysMember(DayInMonth day, IEnumerable<string> availableMembers)
    {
        var memberCounts = _memberCounts
            .Where(r => availableMembers.Contains(r.Key))
            .ToDictionary(r => r.Key, r => r.Value.GetCount(day.IsHoliday));
        var minValue = memberCounts.Min(r => r.Value);
        return memberCounts.Where(r => r.Value == minValue).Select(r => r.Key).ToArray();
    }

    private string RandomMember(IReadOnlyList<string> leastDaysMembers)
    {
        var next = _random.Next(leastDaysMembers.Count - 1);
        return leastDaysMembers[next];
    }
}
