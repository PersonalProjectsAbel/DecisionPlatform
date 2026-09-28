using DecisionPlanner.Application.Meeting.Projections;
using DecisionPlanner.Domain.Meeting.Events;
using Microsoft.Extensions.Logging;

namespace DecisionPlanner.Infrastructure.Projections.Meeting;

public sealed class ParticipantAddedProjectionHandler(
    IMeetingReadModelRepository repository,
    ILogger<ParticipantAddedProjectionHandler> logger)
    : MeetingProjectionHandlerBase<ParticipantAddedEvent>(logger)
{
    public override string EventType => MeetingEventTypes.ParticipantAdded;

    protected override Task ProjectAsync(
        ParticipantAddedEvent @event,
        CancellationToken cancellationToken)
    {
        var readModel = new ParticipantReadModel(
            @event.ParticipantId,
            @event.MeetingId,
            @event.Name,
            @event.Email);

        return repository.UpsertParticipantAsync(readModel, cancellationToken);
    }
}
