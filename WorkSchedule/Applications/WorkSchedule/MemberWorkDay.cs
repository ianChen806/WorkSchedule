namespace WorkSchedule.Applications.WorkSchedule;

public class MemberWorkDay
{
    public string Name { get; set; } = null!;

    public List<DateTime> IgnoreDays { get; set; } = new List<DateTime>();
}
