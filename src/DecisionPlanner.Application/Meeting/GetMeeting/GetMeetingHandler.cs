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
                .ToArray(),
            meeting.Topics
                .Select(topic => new TopicResponse(
                    topic.Id,
                    topic.Title,
                    topic.IsGeneral,
                    topic.Proposals.Select(proposal => new ProposalResponse(
                        proposal.Id, proposal.Title, proposal.Description)).ToArray()))
                .ToArray(),
            meeting.MinutesDrafts
                .Select(draft => new MinutesDraftResponse(
                    draft.CompletedEventId,
                    draft.Content,
                    draft.GeneratedAt))
                .ToArray());
    }
}
