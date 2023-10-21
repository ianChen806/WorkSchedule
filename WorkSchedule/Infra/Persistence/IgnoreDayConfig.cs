using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkSchedule.Domain.Entities;

namespace WorkSchedule.Infra.Persistence;

public class IgnoreDayConfig : IEntityTypeConfiguration<MemberIgnoreDay>
{
    public void Configure(EntityTypeBuilder<MemberIgnoreDay> builder)
    {
        builder.ToTable("MemberIgnoreDay");
        builder.HasKey(r => new { r.MemberId, r.Day });
    }
}
