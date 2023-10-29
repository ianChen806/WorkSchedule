using Microsoft.AspNetCore.Components;
using WorkSchedule.Applications.WorkSchedule;
using WorkSchedule.Domain.ValueObject;

namespace WorkSchedule.Components.Pages;

public class HomeBase : ComponentBase
{
    [Inject]
    public required WorkScheduleHandler Handler { get; set; }

    public WorkMember? Model { get; set; }

    protected List<MemberWorkDay> Members { get; set; } = new();

    protected int Year { get; set; } = 2023;

    protected int Month { get; set; } = 11;

    protected List<WorkScheduleViewModel> Result { get; set; } = new();

    protected override Task OnInitializedAsync()
    {
        Model = new WorkMember();
        return base.OnInitializedAsync();
    }

    protected void Add()
    {
        var name = Model!.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            Model = new WorkMember();
            return;
        }
        if (Members.Any(r => r.Name == name))
        {
            Members.Remove(Members.First(r => r.Name == name));
        }
        Members.Add(new MemberWorkDay
        {
            Name = name ?? "",
            IgnoreDays = Model.IgnoreDays?.Split(',')
                .Select(int.Parse)
                .Select(r => new DateTime(Year, Month, r))
                .ToList() ?? new List<DateTime>()
        });
        Model = new WorkMember();
    }

    protected async Task Submit()
    {
        if (Members.Any() == false)
        {
            return;
        }

        var result = await Handler.Handle(new WorkScheduleCommand
        {
            Date = new DateObject(Year, Month),
            Members = Members.Select(r => new MemberWorkDay
            {
                Name = r.Name,
                IgnoreDays = new List<DateTime>(r.IgnoreDays)
            }).ToList()
        });

        Result = result.ScheduleFirst
            .OrderBy(r => r.Date)
            .Zip(result.ScheduleSecond)
            .Select(r => new WorkScheduleViewModel
            {
                Date = r.First.Date,
                First = r.First.Person,
                Second = r.Second.Person,
                IsHoliday = r.First.IsHoliday
            })
            .ToList();
    }

    public class WorkMember
    {
        public string? Name { get; set; }

        public string? IgnoreDays { get; set; }
    }

    protected void CleanMembers()
    {
        Members = new List<MemberWorkDay>();
    }
}
