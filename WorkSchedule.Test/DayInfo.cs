namespace WorkSchedule.Test;

public record DayInfo
{
    public DayInMonth Day { get; set; }

    public string Person { get; set; }

    public bool IsHoliday { get; set; }
}
