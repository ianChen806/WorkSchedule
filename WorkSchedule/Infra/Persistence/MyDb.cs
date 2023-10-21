using Microsoft.EntityFrameworkCore;
using WorkSchedule.Applications.Common.Interfaces;
using WorkSchedule.Domain.Entities;

namespace WorkSchedule.Infra.Persistence;

public class MyDb : DbContext, IMyDb
{
    public MyDb(DbContextOptions<MyDb> options)
        : base(options)
    {
    }

    public DbSet<Member> Members { get; set; }
}
