using WorkSchedule.Domain.Models;

namespace WorkSchedule.Applications.Common.Interfaces;

public interface IOpenApi
{
    Task<IEnumerable<DayOfMonth>> GetDays(int year, int month);
}