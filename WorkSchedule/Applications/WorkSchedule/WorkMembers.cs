namespace WorkSchedule.Applications.WorkSchedule;

public class WorkMembers
{
    private readonly Dictionary<string, WorkDayCount> _memberCounts;
    private readonly List<MemberWorkDay> _members;
    private readonly Random _random;

    public WorkMembers(List<MemberWorkDay> members)
    {
        _members = members;
        _memberCounts = members.ToDictionary(r => r.Name, _ => new WorkDayCount());
        _random = new Random(Guid.NewGuid().GetHashCode());
    }

    public string GetMember(DateTime date, bool isHoliday)
    {
        var availableMembers = AvailableMembers(date);
        var leastDaysMembers = LeastDaysMember(availableMembers, isHoliday);
        var member = RandomMember(leastDaysMembers);
        IncreaseMemberDays(isHoliday, member);
        return member;
    }

    public WorkMembers SetIgnoreDays(List<DayInMonth> schedule)
    {
        foreach (var member in _members)
        {
            var ignoreDays = schedule.Where(r => r.Person == member.Name);
            member.IgnoreDays.AddRange(ignoreDays.Select(r => r.Date));
        }
        return this;
    }

    private IEnumerable<string> AvailableMembers(DateTime date)
    {
        return _members.Where(r => r.IgnoreDays.TrueForAll(s => s != date)).Select(r => r.Name);
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

    private string[] LeastDaysMember(IEnumerable<string> availableMembers, bool isHoliday)
    {
        var memberCounts = _memberCounts
            .Where(r => availableMembers.Contains(r.Key))
            .ToDictionary(r => r.Key, r => r.Value.GetCount(isHoliday));
        var minValue = memberCounts.Min(r => r.Value);
        return memberCounts.Where(r => r.Value == minValue).Select(r => r.Key).ToArray();
    }

    private string RandomMember(IReadOnlyList<string> leastDaysMembers)
    {
        var next = _random.Next(leastDaysMembers.Count - 1);
        return leastDaysMembers[next];
    }
}
