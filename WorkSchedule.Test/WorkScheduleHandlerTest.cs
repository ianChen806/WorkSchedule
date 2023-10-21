using FluentAssertions;
using NSubstitute;

namespace WorkSchedule.Test;

public class WorkScheduleHandlerTest
{
    private readonly WorkScheduleHandler _target;
    private readonly TimeProvider _timeProvider;
    private const int PeopleCount = 5;

    public WorkScheduleHandlerTest()
    {
        _timeProvider = Substitute.For<TimeProvider>();
        _target = new WorkScheduleHandler(_timeProvider);
    }

    [Fact]
    public async Task 安排天數平均分配給5個人()
    {
        GivenUtcNow();
        var actual = await _target.Handle(new WorkScheduleCommand());
        ShouldAverageDaysForPeople(actual);
    }

    private void ShouldAverageDaysForPeople(WorkScheduleResult actual)
    {
        (actual.Schedule.GroupBy(r => r.Person).Select(r => r.Count()).Sum() / PeopleCount)
            .Should().Be(6);
    }

    private void GivenUtcNow()
    {
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2023, 9, 1, 0, 0, 0, TimeSpan.Zero));
    }
}
