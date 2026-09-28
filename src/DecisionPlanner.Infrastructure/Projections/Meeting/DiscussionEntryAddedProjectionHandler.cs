using DecisionPlanner.Application.Meeting.Projections;
using DecisionPlanner.Domain.Meeting.Events;
using Microsoft.Extensions.Logging;

namespace DecisionPlanner.Infrastructure.Projections.Meeting;

public sealed class DiscussionEntryAddedProjectionHandler(
    IMeetingReadModelRepository repository,
    ILogger<DiscussionEntryAddedProjectionHandler> logger)
    : MeetingProjectionHandlerBase<DiscussionEntryAddedEvent>(logger)
{
    public override string EventType => MeetingEventTypes.DiscussionEntryAdded;

    protected override Task ProjectAsync(
        DiscussionEntryAddedEvent @event, CancellationToken cancellationToken) =>
        repository.UpsertDiscussionEntryAsync(new DiscussionEntryReadModel(
            @event.EntryId,
            @event.MeetingId,
            @event.TopicId,
            string.Empty,
            @event.ProposalId,
            null,
            @event.AuthorParticipantId,
            @event.AuthorName,
            @event.Content,
            @event.CreatedAt), cancellationToken);
}
