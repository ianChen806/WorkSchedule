using WorkSchedule.Applications.Common.Interfaces;

namespace WorkSchedule.Applications.WorkSchedule;

public class WorkScheduleHandler(TimeProvider timeProvider, IOpenApi openApi)
{
    public async Task<WorkScheduleResult> Handle(WorkScheduleCommand request)
    {
        var scheduleFirst = await ScheduleDays(new WorkDay(request.Members));

        var workDay = new WorkDay(request.Members).SetIgnoreDays(scheduleFirst);
        var scheduleSecond = await ScheduleDays(workDay);
        return new WorkScheduleResult
        {
            ScheduleFirst = scheduleFirst,
            ScheduleSecond = scheduleSecond
        };
    }

    private async Task<List<DayInMonth>> GetMonthDays()
    {
        var now = timeProvider.GetLocalNow();
        var dayOfMonths = await openApi.GetDays(now.Year, now.Month);
        return dayOfMonths
            .Select(r => new DayInMonth(r.Date, r.IsHoliday))
            .ToList();
    }

    private async Task<List<DayInMonth>> ScheduleDays(WorkDay workDay)
    {
        var daysInMonth = await GetMonthDays();
        foreach (var day in daysInMonth)
        {
            day.Person = workDay.GetMember(day);
        }
        return daysInMonth;
    }
}
