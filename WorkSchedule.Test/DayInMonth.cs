namespace WorkSchedule.Test;

public class DayInMonth
{
    public DateTime Date { get; }

    public string Person { get; set; }

    public DayInMonth(DateTime date)
    {
        Date = date;
    }
}
