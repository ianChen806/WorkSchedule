using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkSchedule.Domain.Entities;

namespace WorkSchedule.Infra.Persistence.Configs;

public class MemberConfig : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(r => r.Id);
        builder.ToTable("Member");

        builder.HasMany(r => r.IgnoreDays)
            .WithOne(r => r.Member)
            .HasForeignKey(r => r.MemberId);
    }
}
