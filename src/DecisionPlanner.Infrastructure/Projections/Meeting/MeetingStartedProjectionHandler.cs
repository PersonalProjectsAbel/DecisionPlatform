using DecisionPlanner.Application.Meeting.Projections;
using DecisionPlanner.Domain.Meeting.Events;
using Microsoft.Extensions.Logging;

namespace DecisionPlanner.Infrastructure.Projections.Meeting;

public sealed class MeetingStartedProjectionHandler(
    IMeetingReadModelRepository repository,
    ILogger<MeetingStartedProjectionHandler> logger)
    : MeetingProjectionHandlerBase<MeetingStartedEvent>(logger)
{
    public override string EventType => MeetingEventTypes.Started;

    protected override Task ProjectAsync(
        MeetingStartedEvent @event,
        CancellationToken cancellationToken)
    {
        return repository.UpdateMeetingStatusAsync(
            @event.MeetingId, "InProgress", cancellationToken);
    }
}
