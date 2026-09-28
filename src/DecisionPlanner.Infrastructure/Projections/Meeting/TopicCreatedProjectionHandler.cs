using DecisionPlanner.Application.Meeting.Projections;
using DecisionPlanner.Domain.Meeting.Events;
using Microsoft.Extensions.Logging;

namespace DecisionPlanner.Infrastructure.Projections.Meeting;

public sealed class TopicCreatedProjectionHandler(
    IMeetingReadModelRepository repository,
    ILogger<TopicCreatedProjectionHandler> logger)
    : MeetingProjectionHandlerBase<TopicCreatedEvent>(logger)
{
    public override string EventType => MeetingEventTypes.TopicCreated;

    protected override Task ProjectAsync(TopicCreatedEvent @event, CancellationToken cancellationToken) =>
        repository.UpsertTopicAsync(new TopicReadModel(
            @event.TopicId, @event.MeetingId, @event.Title, @event.IsGeneral), cancellationToken);
}
