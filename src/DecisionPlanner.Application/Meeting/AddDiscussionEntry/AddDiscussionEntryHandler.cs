using MediatR;
using DecisionPlanner.Application.Meeting;

namespace DecisionPlanner.Application.Meeting.AddDiscussionEntry;

public sealed class AddDiscussionEntryHandler(IMeetingRepository repository)
    : IRequestHandler<AddDiscussionEntryCommand, Guid?>
{
    public async Task<Guid?> Handle(AddDiscussionEntryCommand command, CancellationToken cancellationToken)
    {
        var meeting = await repository.GetByIdAsync(command.MeetingId, cancellationToken);
        if (meeting is null) return null;
        var entryId = meeting.AddDiscussionEntry(
            command.TopicId, command.ProposalId, command.AuthorParticipantId, command.Content);
        await repository.SaveChangesAsync(meeting, cancellationToken);
        return entryId;
    }
}
