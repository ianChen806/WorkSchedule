using FluentAssertions;
using NSubstitute;
using WorkSchedule.Applications.Common.Interfaces;
using WorkSchedule.Applications.WorkSchedule;
using WorkSchedule.Domain.Models;
using WorkSchedule.Domain.ValueObject;

namespace WorkSchedule.Test;

public class WorkScheduleHandlerTest
{
    private const int PeopleCount = 5;
    private readonly IOpenApi _openApi;
    private readonly WorkScheduleHandler _target;
    private readonly TimeProvider _timeProvider;

    public WorkScheduleHandlerTest()
    {
        _openApi = Substitute.For<IOpenApi>();
        _timeProvider = Substitute.For<TimeProvider>();
        _target = new WorkScheduleHandler(_openApi);
    }

    [Fact]
    public async Task 安排天數平均分配給5個人()
    {
        GivenDays();
        GivenUtcNow();
        var actual = await WhenHandle();
        ShouldAverageEveryone(actual);
    }

    [Fact]
    public void 排除個人忽略的日子()
    {
        GivenDayOfMonthIncludeHolidays();
        var members = GivenMemberIncludeIgnoreDays();
        var workDay = new WorkMembers(members);
        DayShouldIncludeMember(workDay, new DateOnly(2023, 9, 1), "Person2");
        DayShouldIncludeMember(workDay, new DateOnly(2023, 9, 2), "Person2");
        DayShouldIncludeMember(workDay, new DateOnly(2023, 9, 3), "Person1");
    }

    [Fact]
    public async Task 假日跟平日分開()
    {
        GivenUtcNow();
        GivenDayOfMonthIncludeHolidays();
        var actual = await WhenHandle();
        ShouldAverageDaysForPeopleWhen(actual, false, 5);
        ShouldAverageDaysForPeopleWhen(actual, true, 1);
    }

    [Fact]
    public async Task 分配主跟副_不能同日()
    {
        GivenUtcNow();
        GivenDays();
        var actual = await WhenHandle();
        FirstPersonShouldNotSameSecondPerson(actual);
    }

    [Fact(Skip = "尚未完成")]
    public async Task 改變選擇member的邏輯_挑出剩餘天數最少的人()
    {
        throw new NotImplementedException();
    }

    private void ShouldAverageEveryone(WorkScheduleResult actual)
    {
        actual.ScheduleFirst.GroupBy(r => r.Person)
            .Select(r => r.Count())
            .All(r => r == 1)
            .Should()
            .BeTrue();
    }

    private void FirstPersonShouldNotSameSecondPerson(WorkScheduleResult actual)
    {
        var zip = actual.ScheduleFirst.Zip(actual.ScheduleSecond).ToList();
        zip.Should().NotBeEmpty();
        zip.All(r => r.First.Person != r.Second.Person).Should().BeTrue();
    }

    private void GivenDays()
    {
        var dayOfMonths = Enumerable.Range(1, 5).Select(r => new DayOfMonth
        {
            Date = new DateOnly(2023, 9, r),
            IsHoliday = false
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
        var dayOfMonths = Enumerable.Range(1, 30).Select(r => new DayOfMonth
        {
            Date = new DateOnly(2023, 9, r),
            IsHoliday = holidays.Contains(r)
        });
        _openApi.GetDays(2023, 9).Returns(dayOfMonths);
    }

    private void DayShouldIncludeMember(WorkMembers workMembers, DateOnly day, string expected)
    {
        workMembers.GetMember(day.ToDateTime(TimeOnly.MinValue), false)
            .Should()
            .BeEquivalentTo(expected);
    }

    private List<MemberWorkDay> GivenMemberIncludeIgnoreDays()
    {
        return new List<MemberWorkDay>
        {
            new()
            {
                Name = "Person1", IgnoreDays = new List<DateTime>
                {
                    new(2023, 9, 1),
                    new(2023, 9, 2)
                }
            },
            new()
            {
                Name = "Person2", IgnoreDays = new List<DateTime>
                {
                    new(2023, 9, 3)
                }
            }
        };
    }

    private async Task<WorkScheduleResult> WhenHandle()
    {
        return await _target.Handle(new WorkScheduleCommand
        {
            Date = new DateObject(2023, 9),
            Members = new List<MemberWorkDay>
            {
                new() { Name = "Member1" },
                new() { Name = "Member2" },
                new() { Name = "Member3" },
                new() { Name = "Member4" },
                new() { Name = "Member5" }
            }
        });
    }

    private void GivenUtcNow()
    {
        _timeProvider.LocalTimeZone.Returns(TimeZoneInfo.Local);
        _timeProvider.GetUtcNow().Returns(new DateTimeOffset(2023, 9, 1, 0, 0, 0, TimeSpan.Zero));
    }
}
