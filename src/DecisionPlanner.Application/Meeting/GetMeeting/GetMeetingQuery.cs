using MediatR;
using DecisionPlanner.Domain.Meeting;

namespace DecisionPlanner.Application.Meeting.GetMeeting;

public sealed record GetMeetingQuery(MeetingId Id) : IRequest<GetMeetingResponse?>;

public sealed record GetMeetingResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Status,
    IReadOnlyCollection<ParticipantResponse> Participants,
    IReadOnlyCollection<TopicResponse> Topics,
    IReadOnlyCollection<MinutesDraftResponse> MinutesDrafts);

public sealed record ParticipantResponse(
    Guid Id,
    string Name,
    string Email);

public sealed record TopicResponse(
    Guid Id,
    string Title,
    bool IsGeneral,
    IReadOnlyCollection<ProposalResponse> Proposals);

public sealed record ProposalResponse(Guid Id, string Title, string? Description);

public sealed record MinutesDraftResponse(
    Guid CompletedEventId,
    string Content,
    DateTimeOffset GeneratedAt);
