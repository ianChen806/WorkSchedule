using Microsoft.EntityFrameworkCore;
using WorkSchedule.Domain.Entities;

namespace WorkSchedule.Applications.Common.Interfaces;

public interface IMyDb
{
    DbSet<Member> Members { get; set; }
}