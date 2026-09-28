using MediatR;
using DecisionPlanner.Domain.Meeting;

namespace DecisionPlanner.Application.Meeting.CreateTopic;

public sealed record CreateTopicCommand(MeetingId MeetingId, string Title) : IRequest<Guid?>;
