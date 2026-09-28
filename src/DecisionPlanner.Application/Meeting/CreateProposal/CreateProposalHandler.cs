using MediatR;
using DecisionPlanner.Application.Meeting;

namespace DecisionPlanner.Application.Meeting.CreateProposal;

public sealed class CreateProposalHandler(IMeetingRepository repository)
    : IRequestHandler<CreateProposalCommand, Guid?>
{
    public async Task<Guid?> Handle(CreateProposalCommand command, CancellationToken cancellationToken)
    {
        var meeting = await repository.GetByIdAsync(command.MeetingId, cancellationToken);
        if (meeting is null) return null;
        var proposalId = meeting.CreateProposal(command.TopicId, command.Title, command.Description);
        await repository.SaveChangesAsync(meeting, cancellationToken);
        return proposalId;
    }
}
