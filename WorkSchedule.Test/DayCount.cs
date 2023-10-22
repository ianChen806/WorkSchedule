namespace WorkSchedule.Test;

internal class DayCount
{
    public int Holidays { get; set; } = 0;

    public int WorkDays { get; set; } = 0;

    public int GetCount(bool isHoliday) => isHoliday ? Holidays : WorkDays;
}
