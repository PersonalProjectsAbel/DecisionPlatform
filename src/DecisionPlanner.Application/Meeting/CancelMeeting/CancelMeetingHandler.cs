using MediatR;
using DecisionPlanner.Application.Meeting;

namespace DecisionPlanner.Application.Meeting.CancelMeeting;

public sealed class CancelMeetingHandler(IMeetingRepository meetingRepository)
    : IRequestHandler<CancelMeetingCommand, bool>
{
    public async Task<bool> Handle(
        CancelMeetingCommand command,
        CancellationToken cancellationToken)
    {
        var meeting = await meetingRepository.GetByIdAsync(
            command.MeetingId,
            cancellationToken);

        if (meeting is null)
            return false;

        meeting.Cancel();
        await meetingRepository.SaveChangesAsync(meeting, cancellationToken);
        return true;
    }
}