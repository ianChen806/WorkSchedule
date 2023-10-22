namespace WorkSchedule.Applications.WorkSchedule;

public class WorkScheduleResult
{
    public List<DayInMonth> ScheduleFirst { get; set; } = new();

    public List<DayInMonth> ScheduleSecond { get; set; } = new();
}
