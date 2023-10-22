using WorkSchedule.Applications.Common.Interfaces;
using WorkSchedule.Domain.Models;

namespace WorkSchedule.Infra.Apis;

internal class OpenApi(IHttpClientFactory factory) : IOpenApi
{
    public async Task<IEnumerable<DayOfMonth>> GetDays(int year, int month)
    {
        var client = factory.CreateClient();
        var uri = $"https://cdn.jsdelivr.net/gh/ruyut/TaiwanCalendar/data/{year}.json";
        var days = await client.GetFromJsonAsync<List<TaiwanCalendarResult>>(uri);
        return days!.Select(r => new DayOfMonth()
        {
            Date = DateOnly.ParseExact(r.Date, "yyyyMMdd"),
            IsHoliday = r.IsHoliday
        });
    }
}
