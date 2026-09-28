using DecisionPlanner.Application.Meeting.Projections;
using DecisionPlanner.Domain.Meeting.Events;
using Microsoft.Extensions.Logging;

namespace DecisionPlanner.Infrastructure.Projections.Meeting;

public sealed class ProposalCreatedProjectionHandler(
    IMeetingReadModelRepository repository,
    ILogger<ProposalCreatedProjectionHandler> logger)
    : MeetingProjectionHandlerBase<ProposalCreatedEvent>(logger)
{
    public override string EventType => MeetingEventTypes.ProposalCreated;

    protected override Task ProjectAsync(ProposalCreatedEvent @event, CancellationToken cancellationToken) =>
        repository.UpsertProposalAsync(new ProposalReadModel(
            @event.ProposalId, @event.MeetingId, @event.TopicId,
            @event.Title, @event.Description), cancellationToken);
}
