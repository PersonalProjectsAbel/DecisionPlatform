using MediatR;
using DecisionPlanner.Application.Meeting;
using DecisionPlanner.Domain.Meeting;

namespace DecisionPlanner.Application.Meeting.AddParticipant;

public sealed class AddParticipantHandler(IMeetingRepository meetingRepository)
    : IRequestHandler<AddParticipantCommand, bool>
{
    public async Task<bool> Handle(
        AddParticipantCommand command,
        CancellationToken cancellationToken)
    {
        var meeting = await meetingRepository.GetByIdAsync(
            command.MeetingId,
            cancellationToken);

        if (meeting is null)
            return false;

        meeting.AddParticipant(new Participant(
            Guid.NewGuid(),
            command.Name,
            command.Email));

        await meetingRepository.SaveChangesAsync(meeting, cancellationToken);
        return true;
    }
}