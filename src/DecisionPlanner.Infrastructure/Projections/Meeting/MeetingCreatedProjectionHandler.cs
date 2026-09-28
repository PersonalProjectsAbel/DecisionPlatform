using DecisionPlanner.Application.Meeting.Projections;
using DecisionPlanner.Domain.Meeting.Events;
using Microsoft.Extensions.Logging;

namespace DecisionPlanner.Infrastructure.Projections.Meeting;

public sealed class MeetingCreatedProjectionHandler(
    IMeetingReadModelRepository repository,
    ILogger<MeetingCreatedProjectionHandler> logger)
    : MeetingProjectionHandlerBase<MeetingCreatedEvent>(logger)
{
    public override string EventType => MeetingEventTypes.Created;

    protected override Task ProjectAsync(
        MeetingCreatedEvent @event,
        CancellationToken cancellationToken)
    {
        var readModel = new MeetingReadModel(
            @event.MeetingId,
            @event.Title,
            @event.Description,
            @event.StartsAt,
            @event.EndsAt,
            "Scheduled");

        return repository.UpsertMeetingAsync(readModel, cancellationToken);
    }
}
