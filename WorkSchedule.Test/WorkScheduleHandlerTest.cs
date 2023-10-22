using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using WorkSchedule.Applications.Common.Interfaces;
using WorkSchedule.Domain.Entities;
using WorkSchedule.Domain.Models;
using WorkSchedule.Infra.Persistence;
using Xunit.Abstractions;

namespace WorkSchedule.Test;

public class WorkScheduleHandlerTest
{
    private const int PeopleCount = 5;
    private readonly MyDb _db;
    private readonly WorkScheduleHandler _target;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly TimeProvider _timeProvider;
    private IOpenApi _openApi;

    public WorkScheduleHandlerTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _openApi = Substitute.For<IOpenApi>();

        _timeProvider = Substitute.For<TimeProvider>();
        _db = TestDbHelper.NewDb();
        _target = new WorkScheduleHandler(_timeProvider, _db, _openApi);
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
        DayShouldIncludeMember(workDay, new DateTime(2023, 10, 1), "Person2");
        DayShouldIncludeMember(workDay, new DateTime(2023, 10, 2), "Person2");
        DayShouldIncludeMember(workDay, new DateTime(2023, 10, 3), "Person1");
    }

    [Fact]
    public async Task 假日跟平日分開()
    {
        GivenUtcNow();
        GivenDayOfMonth();
        var actual = await WhenHandle();
        ShouldAverageDaysForPeopleWhen(actual, false, 4);
        ShouldAverageDaysForPeopleWhen(actual, true, 1);
    }

    private void ShouldAverageDaysForPeopleWhen(WorkScheduleResult actual, bool isHoliday, int expected)
    {
        (actual.Schedule.Where(r => r.IsHoliday == isHoliday)
                .GroupBy(r => r.Person)
                .Select(r => r.Count()).Sum() / PeopleCount)
            .Should().Be(expected);
    }

    private void GivenDayOfMonth()
    {
        var holidays = new List<int> { 1, 2, 8, 9, 15 };
        var dayOfMonths = Enumerable.Range(1, 30).Select(r => new DayOfMonth()
        {
            Date = new DateOnly(2023, 10, r),
            IsHoliday = holidays.Contains(r),
        });
        _openApi.GetDays(2023, 10).Returns(dayOfMonths);
    }

    private void DayShouldIncludeMember(WorkDay workDay, DateTime day, string expected)
    {
        workDay.Members(new DayInMonth(day))
            .Should()
            .BeEquivalentTo(new[] { new { Name = expected } });
    }

    private List<MemberWorkDay> GivenMemberIncludeIgnoreDays()
    {
        return new List<MemberWorkDay>()
        {
            new()
            {
                Name = "Person1", IgnoreDays = new List<DateTime>()
                {
                    new(2023, 10, 1),
                    new(2023, 10, 2),
                },
            },
            new()
            {
                Name = "Person2", IgnoreDays = new List<DateTime>()
                {
                    new(2023, 10, 3),
                },
            }
        };
    }

    private async Task<WorkScheduleResult> WhenHandle()
    {
        return await _target.Handle(new WorkScheduleCommand());
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
