using MediatR;
using DecisionPlanner.Application.Meeting.Projections;

namespace DecisionPlanner.Application.Meeting.GetDiscussionEntries;

public sealed class GetDiscussionEntriesHandler(IMeetingReadModelRepository repository)
    : IRequestHandler<GetDiscussionEntriesQuery, IReadOnlyList<DiscussionEntryResponse>>
{
    public async Task<IReadOnlyList<DiscussionEntryResponse>> Handle(
        GetDiscussionEntriesQuery query, CancellationToken cancellationToken)
    {
        var entries = await repository.GetDiscussionEntriesAsync(
            query.MeetingId, query.TopicId, query.ProposalId, cancellationToken);
        return entries.Select(entry => new DiscussionEntryResponse(
            entry.Id, entry.TopicId, entry.TopicTitle, entry.ProposalId,
            entry.ProposalTitle, entry.AuthorParticipantId, entry.AuthorName,
            entry.Content, entry.CreatedAt)).ToArray();
    }
}
