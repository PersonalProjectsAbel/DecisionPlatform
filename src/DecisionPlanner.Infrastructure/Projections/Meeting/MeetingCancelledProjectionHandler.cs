using DecisionPlanner.Application.Meeting.Projections;
using DecisionPlanner.Domain.Meeting.Events;
using Microsoft.Extensions.Logging;

namespace DecisionPlanner.Infrastructure.Projections.Meeting;

public sealed class MeetingCancelledProjectionHandler(
    IMeetingReadModelRepository repository,
    ILogger<MeetingCancelledProjectionHandler> logger)
    : MeetingProjectionHandlerBase<MeetingCancelledEvent>(logger)
{
    public override string EventType => MeetingEventTypes.Cancelled;

    protected override Task ProjectAsync(
        MeetingCancelledEvent @event,
        CancellationToken cancellationToken)
    {
        return repository.UpdateMeetingStatusAsync(
            @event.MeetingId, "Cancelled", cancellationToken);
    }
}
