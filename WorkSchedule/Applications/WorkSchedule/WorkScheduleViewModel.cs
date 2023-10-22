namespace WorkSchedule.Applications.WorkSchedule;

internal class WorkScheduleViewModel
{
    public string Day { get; set; } = null!;

    public string? First { get; set; }

    public string? Second { get; set; }

    public bool IsHoliday { get; set; }
}