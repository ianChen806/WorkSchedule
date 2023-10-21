using Microsoft.EntityFrameworkCore;
using WorkSchedule.Applications.Common.Interfaces;
using WorkSchedule.Domain.Entities;
using WorkSchedule.Infra.Persistence.Configs;

namespace WorkSchedule.Infra.Persistence;

public class MyDb : DbContext, IMyDb
{
    public MyDb(DbContextOptions<MyDb> options)
        : base(options)
    {
    }

    public DbSet<Member> Members { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MemberConfig());
        modelBuilder.ApplyConfiguration(new IgnoreDayConfig());
        base.OnModelCreating(modelBuilder);
    }
}