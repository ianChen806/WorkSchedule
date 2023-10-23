using WorkSchedule.Applications.Common.Interfaces;
using WorkSchedule.Domain.ValueObject;

namespace WorkSchedule.Applications.WorkSchedule;

public class WorkScheduleHandler(IOpenApi openApi)
{
    public async Task<WorkScheduleResult> Handle(WorkScheduleCommand request)
    {
        var scheduleFirst = await ScheduleDays(new WorkMembers(request.Members), request.Date);

        var workDay = new WorkMembers(request.Members).SetIgnoreDays(scheduleFirst);
        var scheduleSecond = await ScheduleDays(workDay, request.Date);
        return new WorkScheduleResult
        {
            ScheduleFirst = scheduleFirst,
            ScheduleSecond = scheduleSecond
        };
    }

    private async Task<List<DayInMonth>> GetMonthDays(DateObject date)
    {
        var dayOfMonths = await openApi.GetDays(date.Year, date.Month);
        return dayOfMonths
            .Select(r => new DayInMonth(r.Date, r.IsHoliday))
            .ToList();
    }

    private async Task<List<DayInMonth>> ScheduleDays(WorkMembers workMembers, DateObject date)
    {
        var daysInMonth = await GetMonthDays(date);
        foreach (var day in daysInMonth)
        {
            day.SetPerson(workMembers);
        }
        return daysInMonth;
    }
}
