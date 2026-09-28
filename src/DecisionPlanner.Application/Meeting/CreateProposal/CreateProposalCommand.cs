using MediatR;
using DecisionPlanner.Domain.Meeting;

namespace DecisionPlanner.Application.Meeting.CreateProposal;

public sealed record CreateProposalCommand(
    MeetingId MeetingId, Guid TopicId, string Title, string? Description) : IRequest<Guid?>;
