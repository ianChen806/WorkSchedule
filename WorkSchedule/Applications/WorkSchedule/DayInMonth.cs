namespace WorkSchedule.Applications.WorkSchedule;

public class DayInMonth
{
    public DayInMonth(DateOnly date, bool isHoliday)
    {
        Date = date.ToDateTime(TimeOnly.MinValue);
        IsHoliday = isHoliday;
    }

    public DateTime Date { get; }

    public bool IsHoliday { get; set; }

    public string? Person { get; set; }

    public void SetPerson(WorkMembers workMembers)
    {
        Person = workMembers.GetMember(this);
    }
}
