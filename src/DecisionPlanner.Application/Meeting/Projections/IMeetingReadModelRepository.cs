namespace DecisionPlanner.Application.Meeting.Projections;

public interface IMeetingReadModelRepository
{
    Task UpsertMeetingAsync(
        MeetingReadModel meeting,
        CancellationToken cancellationToken);

    Task UpsertParticipantAsync(
        ParticipantReadModel participant,
        CancellationToken cancellationToken);

    Task UpdateMeetingStatusAsync(
        Guid meetingId,
        string status,
        CancellationToken cancellationToken);
}
