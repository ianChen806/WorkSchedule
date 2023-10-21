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
        GivenUtcNow();
        GivenMembers();
        var actual = await WhenHandle();
        ShouldAverageDaysForPeople(actual);
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(actual.Schedule));
    }

    [Fact]
    public void 排除個人忽略的日子()
    {
        var members = GivenMemberIncludeIgnoreDays();
        var workDay = new WorkDay(members);
        workDay.Members(new DateTime(2023, 10, 1)).Should().BeEquivalentTo("Person2");
        workDay.Members(new DateTime(2023, 10, 2)).Should().BeEquivalentTo("Person2");
        workDay.Members(new DateTime(2023, 10, 3)).Should().BeEquivalentTo("Person1");
    }

    private List<Member> GivenMemberIncludeIgnoreDays()
    {
        var members = new List<Member>()
        {
            new Member()
            {
                Name = "Person1", IgnoreDays = new List<MemberIgnoreDay>()
                {
                    new MemberIgnoreDay() { Day = new DateTime(2023, 10, 1) },
                    new MemberIgnoreDay() { Day = new DateTime(2023, 10, 2) },
                },
            },
            new Member()
            {
                Name = "Person2", IgnoreDays = new List<MemberIgnoreDay>()
                {
                    new MemberIgnoreDay() { Day = new DateTime(2023, 10, 3) },
                },
            }
        };
        return members;
    }

    private async Task<WorkScheduleResult> WhenHandle()
    {
        return await _target.Handle(new WorkScheduleCommand());
    }

    private void GivenMembersIncludeIgnoreDays()
    {
        _db.Members.Add(new Member()
        {
            Name = "Person1", IgnoreDays = new List<MemberIgnoreDay>()
            {
                new MemberIgnoreDay() { Day = new DateTime(2023, 10, 1) },
                new MemberIgnoreDay() { Day = new DateTime(2023, 10, 2) },
            },
        });
        _db.Members.Add(new Member()
        {
            Name = "Person2", IgnoreDays = new List<MemberIgnoreDay>()
            {
                new MemberIgnoreDay() { Day = new DateTime(2023, 10, 3) },
            },
        });
        _db.SaveChanges();
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
