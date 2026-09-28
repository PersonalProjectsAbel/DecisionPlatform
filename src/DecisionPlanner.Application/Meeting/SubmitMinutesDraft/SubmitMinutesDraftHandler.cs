using DecisionPlanner.Application.Meeting;
using DecisionPlanner.Domain.Meeting;
using MediatR;

namespace DecisionPlanner.Application.Meeting.SubmitMinutesDraft;

public sealed class SubmitMinutesDraftHandler(IMeetingRepository meetingRepository)
    : IRequestHandler<SubmitMinutesDraftCommand, SubmitMinutesDraftResult>
{
    public async Task<SubmitMinutesDraftResult> Handle(
        SubmitMinutesDraftCommand command,
        CancellationToken cancellationToken)
    {
        var meeting = await meetingRepository.GetByIdAsync(
            command.MeetingId,
            cancellationToken);
        if (meeting is null)
            return SubmitMinutesDraftResult.MeetingNotFound;
        if (meeting.Status != MeetingStatus.Completed)
            return SubmitMinutesDraftResult.MeetingNotCompleted;

        var wasRecorded = meeting.RecordMinutesDraft(
            command.CompletedEventId,
            command.Content,
            command.GeneratedAt);
        if (!wasRecorded)
            return SubmitMinutesDraftResult.AlreadyStored;

        await meetingRepository.SaveChangesAsync(meeting, cancellationToken);
        return SubmitMinutesDraftResult.Stored;
    }
}
