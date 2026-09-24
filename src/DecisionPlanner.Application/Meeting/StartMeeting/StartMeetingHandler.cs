using MediatR;
using DecisionPlanner.Application.Meeting;

namespace DecisionPlanner.Application.Meeting.StartMeeting;

public sealed class StartMeetingHandler(IMeetingRepository meetingRepository)
    : IRequestHandler<StartMeetingCommand, bool>
{
    public async Task<bool> Handle(
        StartMeetingCommand command,
        CancellationToken cancellationToken)
    {
        var meeting = await meetingRepository.GetByIdAsync(
            command.MeetingId,
            cancellationToken);

        if (meeting is null)
            return false;

        meeting.Start();
        await meetingRepository.SaveChangesAsync(meeting, cancellationToken);
        return true;
    }
}