using MediatR;
using DecisionPlanner.Domain.Meeting;

namespace DecisionPlanner.Application.Meeting.CompleteMeeting;

public sealed record CompleteMeetingCommand(MeetingId MeetingId) : IRequest<bool>;