using MediatR;
using DecisionPlanner.Application.Meeting;

namespace DecisionPlanner.Application.Meeting.CreateTopic;

public sealed class CreateTopicHandler(IMeetingRepository repository)
    : IRequestHandler<CreateTopicCommand, Guid?>
{
    public async Task<Guid?> Handle(CreateTopicCommand command, CancellationToken cancellationToken)
    {
        var meeting = await repository.GetByIdAsync(command.MeetingId, cancellationToken);
        if (meeting is null) return null;
        var topicId = meeting.CreateTopic(command.Title);
        await repository.SaveChangesAsync(meeting, cancellationToken);
        return topicId;
    }
}
