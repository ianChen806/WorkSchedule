namespace WorkSchedule.Domain.Models;

public class DayOfMonth
{
    public DateOnly Date { get; set; }

    public bool IsHoliday { get; set; }
}
