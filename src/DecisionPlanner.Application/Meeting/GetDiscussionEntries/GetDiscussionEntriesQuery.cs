using MediatR;

namespace DecisionPlanner.Application.Meeting.GetDiscussionEntries;

public sealed record GetDiscussionEntriesQuery(
    Guid MeetingId, Guid? TopicId, Guid? ProposalId) : IRequest<IReadOnlyList<DiscussionEntryResponse>>;

public sealed record DiscussionEntryResponse(
    Guid Id,
    Guid TopicId,
    string TopicTitle,
    Guid? ProposalId,
    string? ProposalTitle,
    Guid AuthorParticipantId,
    string AuthorName,
    string Content,
    DateTimeOffset CreatedAt);
