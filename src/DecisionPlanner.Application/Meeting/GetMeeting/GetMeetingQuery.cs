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
    IReadOnlyCollection<ParticipantResponse> Participants);

public sealed record ParticipantResponse(
    Guid Id,
    string Name,
    string Email);