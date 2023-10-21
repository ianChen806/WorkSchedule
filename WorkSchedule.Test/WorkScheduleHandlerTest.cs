using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;

namespace WorkSchedule.Test;

public class WorkScheduleHandlerTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly WorkScheduleHandler _target;
    private readonly TimeProvider _timeProvider;
    private const int PeopleCount = 5;

    public WorkScheduleHandlerTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _timeProvider = Substitute.For<TimeProvider>();
        _target = new WorkScheduleHandler(_timeProvider);
    }

    [Fact]
    public async Task 安排天數平均分配給5個人()
    {
        GivenUtcNow();
        var actual = await _target.Handle(new WorkScheduleCommand());
        ShouldAverageDaysForPeople(actual);
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(actual.Schedule));
    }

    private void ShouldAverageDaysForPeople(WorkScheduleResult actual)
    {
        var enumerable = actual.Schedule.GroupBy(r => r.Person).Select(r => r.Count());
        (enumerable.Sum() / PeopleCount)
            .Should().Be(6);
    }

    private void GivenUtcNow()
    {
        _timeProvider.LocalTimeZone.Returns(TimeZoneInfo.Local);
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2023, 9, 1, 0, 0, 0, TimeSpan.Zero));
    }
}
