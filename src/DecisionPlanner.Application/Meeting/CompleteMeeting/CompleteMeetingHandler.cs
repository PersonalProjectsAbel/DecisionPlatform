using MediatR;
using DecisionPlanner.Application.Meeting;

namespace DecisionPlanner.Application.Meeting.CompleteMeeting;

public sealed class CompleteMeetingHandler(IMeetingRepository meetingRepository)
    : IRequestHandler<CompleteMeetingCommand, bool>
{
    public async Task<bool> Handle(
        CompleteMeetingCommand command,
        CancellationToken cancellationToken)
    {
        var meeting = await meetingRepository.GetByIdAsync(
            command.MeetingId,
            cancellationToken);

        if (meeting is null)
            return false;

        meeting.Complete();
        await meetingRepository.SaveChangesAsync(meeting, cancellationToken);
        return true;
    }
}