using DecisionPlanner.Domain.Meeting;
using MediatR;

namespace DecisionPlanner.Application.Meeting.SubmitMinutesDraft;

public sealed record SubmitMinutesDraftCommand(
    MeetingId MeetingId,
    Guid CompletedEventId,
    string Content,
    DateTimeOffset GeneratedAt) : IRequest<SubmitMinutesDraftResult>;

public enum SubmitMinutesDraftResult
{
    Stored,
    AlreadyStored,
    MeetingNotFound,
    MeetingNotCompleted
}
