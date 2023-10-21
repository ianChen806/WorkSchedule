using System.Collections;
using Microsoft.EntityFrameworkCore;
using WorkSchedule.Domain.Entities;

namespace WorkSchedule.Test;

public class WorkDay
{
    private readonly List<Member> _members;

    public WorkDay(List<Member> members)
    {
        _members = members;
    }

    public List<string> Members(DateTime day)
    {
        return _members.Where(r => r.IgnoreDays.All(s => s.Day != day))
            .Select(r => r.Name)
            .ToList();
    }

}
