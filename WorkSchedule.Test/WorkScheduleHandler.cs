namespace WorkSchedule.Test;

public class WorkScheduleHandler
{
    private readonly TimeProvider _timeProvider;

    public WorkScheduleHandler(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Task<WorkScheduleResult> Handle(WorkScheduleCommand request)
    {
        throw new NotImplementedException();
    }
}