namespace DecisionPlanner.Application.Meeting.Projections;

public interface IMeetingReadModelRepository
{
    Task UpsertMeetingAsync(
        MeetingReadModel meeting,
        CancellationToken cancellationToken);

    Task UpsertParticipantAsync(
        ParticipantReadModel participant,
        CancellationToken cancellationToken);

    Task UpsertTopicAsync(TopicReadModel topic, CancellationToken cancellationToken);

    Task UpsertProposalAsync(ProposalReadModel proposal, CancellationToken cancellationToken);

    Task UpsertDiscussionEntryAsync(
        DiscussionEntryReadModel entry,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DiscussionEntryReadModel>> GetDiscussionEntriesAsync(
        Guid meetingId,
        Guid? topicId,
        Guid? proposalId,
        CancellationToken cancellationToken);

    Task UpdateMeetingStatusAsync(
        Guid meetingId,
        string status,
        CancellationToken cancellationToken);
}
