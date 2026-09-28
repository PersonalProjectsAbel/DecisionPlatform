using DecisionPlanner.Application.Meeting.Projections;
using DecisionPlanner.Domain.Meeting.Events;
using Microsoft.Extensions.Logging;

namespace DecisionPlanner.Infrastructure.Projections.Meeting;

public sealed class MeetingCompletedProjectionHandler(
    IMeetingReadModelRepository repository,
    ILogger<MeetingCompletedProjectionHandler> logger)
    : MeetingProjectionHandlerBase<MeetingCompletedEvent>(logger)
{
    public override string EventType => MeetingEventTypes.Completed;

    protected override Task ProjectAsync(
        MeetingCompletedEvent @event,
        CancellationToken cancellationToken)
    {
        return repository.UpdateMeetingStatusAsync(
            @event.MeetingId, "Completed", cancellationToken);
    }
}
