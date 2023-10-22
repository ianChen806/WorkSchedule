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
    private readonly IOpenApi _openApi;
    private readonly WorkScheduleHandler _target;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly TimeProvider _timeProvider;

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
        GivenDayOfMonths();
        GivenUtcNow();
        GivenMembers();
        var actual = await WhenHandle();
        actual.ScheduleFirst.GroupBy(r => r.Person)
            .Select(r => r.Count())
            .All(r => r == 1)
            .Should()
            .BeTrue();
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(actual.ScheduleFirst));
    }

    [Fact]
    public void 排除個人忽略的日子()
    {
        GivenDayOfMonthIncludeHolidays();
        var members = GivenMemberIncludeIgnoreDays();
        var workDay = new WorkDay(members);
        DayShouldIncludeMember(workDay, new DateOnly(2023, 9, 1), "Person2");
        DayShouldIncludeMember(workDay, new DateOnly(2023, 9, 2), "Person2");
        DayShouldIncludeMember(workDay, new DateOnly(2023, 9, 3), "Person1");
    }

    [Fact]
    public async Task 假日跟平日分開()
    {
        GivenUtcNow();
        GivenDayOfMonthIncludeHolidays();
        GivenMembers();
        var actual = await WhenHandle();
        ShouldAverageDaysForPeopleWhen(actual, false, 5);
        ShouldAverageDaysForPeopleWhen(actual, true, 1);
    }

    [Fact]
    public async Task 分配主跟副_不能同日()
    {
        GivenUtcNow();
        GivenDayOfMonths();
        GivenMembers();
        var actual = await WhenHandle();
        FirstPersonShouldNotSameSecondPerson(actual);
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(actual.ScheduleFirst));
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(actual.ScheduleSecond));
    }

    private void FirstPersonShouldNotSameSecondPerson(WorkScheduleResult actual)
    {
        var zip = actual.ScheduleFirst.Zip(actual.ScheduleSecond).ToList();
        zip.Should().NotBeEmpty();
        zip.All(r => r.First.Person != r.Second.Person).Should().BeTrue();
    }

    private void GivenDayOfMonths()
    {
        var dayOfMonths = Enumerable.Range(1, 5).Select(r => new DayOfMonth()
        {
            Date = new DateOnly(2023, 9, r),
            IsHoliday = false,
        });
        _openApi.GetDays(2023, 9).Returns(dayOfMonths);
    }

    private void ShouldAverageDaysForPeopleWhen(WorkScheduleResult actual, bool isHoliday, int expected)
    {
        var memberDayCounts = actual.ScheduleFirst.Where(r => r.IsHoliday == isHoliday)
            .GroupBy(r => r.Person)
            .Select(r => r.Count());
        (memberDayCounts.Sum() / PeopleCount).Should().Be(expected);
    }

    private void GivenDayOfMonthIncludeHolidays()
    {
        var holidays = new List<int> { 1, 2, 8, 9, 15 };
        var dayOfMonths = Enumerable.Range(1, 30).Select(r => new DayOfMonth()
        {
            Date = new DateOnly(2023, 9, r),
            IsHoliday = holidays.Contains(r),
        });
        _openApi.GetDays(2023, 9).Returns(dayOfMonths);
    }

    private void DayShouldIncludeMember(WorkDay workDay, DateOnly day, string expected)
    {
        workDay.RandomMember(new DayInMonth(day, false))
            .Should()
            .BeEquivalentTo(expected);
    }

    private List<MemberWorkDay> GivenMemberIncludeIgnoreDays()
    {
        return new List<MemberWorkDay>()
        {
            new()
            {
                Name = "Person1", IgnoreDays = new List<DateTime>()
                {
                    new(2023, 9, 1),
                    new(2023, 9, 2),
                },
            },
            new()
            {
                Name = "Person2", IgnoreDays = new List<DateTime>()
                {
                    new(2023, 9, 3),
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
        (actual.ScheduleFirst.GroupBy(r => r.Person).Select(r => r.Count()).Sum() / PeopleCount)
            .Should().Be(6);
    }

    private void GivenUtcNow()
    {
        _timeProvider.LocalTimeZone.Returns(TimeZoneInfo.Local);
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2023, 9, 1, 0, 0, 0, TimeSpan.Zero));
    }
}
