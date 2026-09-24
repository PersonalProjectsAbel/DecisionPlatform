using MediatR;
using DecisionPlanner.Domain.Meeting;

namespace DecisionPlanner.Application.Meeting.CreateMeeting;

public sealed record CreateMeetingCommand(
    string Title,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string? Description = null) : IRequest<MeetingId>;