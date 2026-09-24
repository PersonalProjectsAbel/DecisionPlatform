using MediatR;
using DecisionPlanner.Application.Meeting;

namespace DecisionPlanner.Application.Meeting.GetMeeting;

public sealed class GetMeetingHandler(IMeetingRepository meetingRepository)
    : IRequestHandler<GetMeetingQuery, GetMeetingResponse?>
{
    public async Task<GetMeetingResponse?> Handle(
        GetMeetingQuery query,
        CancellationToken cancellationToken)
    {
        var meeting = await meetingRepository.GetByIdAsync(query.Id, cancellationToken);

        if (meeting is null)
            return null;

        return new GetMeetingResponse(
            meeting.Id.Value,
            meeting.Title,
            meeting.Description,
            meeting.StartsAt,
            meeting.EndsAt,
            meeting.Status.ToString(),
            meeting.Participants
                .Select(participant => new ParticipantResponse(
                    participant.Id,
                    participant.Name,
                    participant.Email))
                .ToArray());
    }
}