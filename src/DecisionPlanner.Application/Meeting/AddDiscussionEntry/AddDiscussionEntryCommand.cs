using MediatR;
using DecisionPlanner.Domain.Meeting;

namespace DecisionPlanner.Application.Meeting.AddDiscussionEntry;

public sealed record AddDiscussionEntryCommand(
    MeetingId MeetingId,
    Guid TopicId,
    Guid? ProposalId,
    Guid AuthorParticipantId,
    string Content) : IRequest<Guid?>;
