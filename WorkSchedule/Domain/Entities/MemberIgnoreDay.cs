namespace WorkSchedule.Domain.Entities;

public class MemberIgnoreDay
{
    public int MemberId { get; set; }

    public DateTime Day { get; set; }

    public virtual Member Member { get; set; }
}
