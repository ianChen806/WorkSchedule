namespace WorkSchedule.Domain.Entities;

public class Member
{
    public int Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<MemberIgnoreDay> IgonoreDays { get; set; }
}
