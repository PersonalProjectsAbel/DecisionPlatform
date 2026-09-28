namespace DecisionPlanner.Domain.Meeting;

public sealed class DiscussionEntry
{
    public Guid Id { get; private set; }
    public Guid? ProposalId { get; private set; }
    public Guid AuthorParticipantId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    private DiscussionEntry() { }

    internal DiscussionEntry(
        Guid id,
        Guid? proposalId,
        Guid authorParticipantId,
        string content,
        DateTimeOffset createdAt)
    {
        Id = id;
        ProposalId = proposalId;
        AuthorParticipantId = authorParticipantId;
        Content = content;
        CreatedAt = createdAt;
    }
}
