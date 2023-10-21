using System.Collections;
using Microsoft.EntityFrameworkCore;
using WorkSchedule.Domain.Entities;

namespace WorkSchedule.Test;

public class WorkDay
{
    private readonly List<MemberWorkDay> _members;

    public WorkDay(List<MemberWorkDay> members)
    {
        _members = members;
    }

    public List<MemberWorkDay> Members(DateTime day)
    {
        return _members.Where(r => r.IgnoreDays.TrueForAll(s => s != day)).ToList();
    }
}
