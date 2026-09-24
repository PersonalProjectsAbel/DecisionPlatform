using MediatR;
using DecisionPlanner.Domain.Meeting;

namespace DecisionPlanner.Application.Meeting.CancelMeeting;

public sealed record CancelMeetingCommand(MeetingId MeetingId) : IRequest<bool>;