using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using WorkSchedule.Domain.Entities;
using WorkSchedule.Infra.Persistence;
using Xunit.Abstractions;

namespace WorkSchedule.Test;

public class WorkScheduleHandlerTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly WorkScheduleHandler _target;
    private readonly TimeProvider _timeProvider;
    private readonly MyDb _db;
    private const int PeopleCount = 5;

    public WorkScheduleHandlerTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _timeProvider = Substitute.For<TimeProvider>();
        _db = TestDbHelper.NewDb();
        _target = new WorkScheduleHandler(_timeProvider, _db);
    }

    [Fact]
    public async Task 安排天數平均分配給5個人()
    {
        GivenMembers();
        GivenUtcNow();
        var actual = await _target.Handle(new WorkScheduleCommand());
        ShouldAverageDaysForPeople(actual);
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(actual.Schedule));
    }

    private void GivenMembers()
    {
        for(int index = 1; index <= 5; index++)
        {
            _db.Members.Add(new Member() { Name = $"Person{index}" });
        }
        _db.SaveChanges();
    }

    private void ShouldAverageDaysForPeople(WorkScheduleResult actual)
    {
        (actual.Schedule.GroupBy(r => r.Person).Select(r => r.Count()).Sum() / PeopleCount)
            .Should().Be(6);
    }

    private void GivenUtcNow()
    {
        _timeProvider.LocalTimeZone.Returns(TimeZoneInfo.Local);
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2023, 9, 1, 0, 0, 0, TimeSpan.Zero));
    }
}
