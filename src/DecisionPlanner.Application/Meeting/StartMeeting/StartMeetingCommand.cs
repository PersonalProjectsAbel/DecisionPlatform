using MediatR;
using DecisionPlanner.Domain.Meeting;

namespace DecisionPlanner.Application.Meeting.StartMeeting;

public sealed record StartMeetingCommand(MeetingId MeetingId) : IRequest<bool>;