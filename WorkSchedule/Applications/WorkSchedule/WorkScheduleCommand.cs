using WorkSchedule.Domain.ValueObject;

namespace WorkSchedule.Applications.WorkSchedule;

public class WorkScheduleCommand
{
    public DateObject Date { get; set; }

    public List<MemberWorkDay> Members { get; set; } = new();
}
