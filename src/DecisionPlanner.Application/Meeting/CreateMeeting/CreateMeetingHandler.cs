using MediatR;
using DecisionPlanner.Application.Meeting;
using DecisionPlanner.Domain.Meeting;
using MeetingAggregate = DecisionPlanner.Domain.Meeting.Meeting;

namespace DecisionPlanner.Application.Meeting.CreateMeeting;

public sealed class CreateMeetingHandler(IMeetingRepository meetingRepository)
    : IRequestHandler<CreateMeetingCommand, MeetingId>
{
    public async Task<MeetingId> Handle(
        CreateMeetingCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var meeting = MeetingAggregate.Create(
            command.Title,
            command.StartsAt,
            command.EndsAt,
            command.Description);

        await meetingRepository.SaveChangesAsync(meeting, cancellationToken);

        return meeting.Id;
    }
}