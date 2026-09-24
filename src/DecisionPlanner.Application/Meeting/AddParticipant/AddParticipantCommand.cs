using MediatR;
using DecisionPlanner.Domain.Meeting;

namespace DecisionPlanner.Application.Meeting.AddParticipant;

public sealed record AddParticipantCommand(
    MeetingId MeetingId,
    string Name,
    string Email) : IRequest<bool>;