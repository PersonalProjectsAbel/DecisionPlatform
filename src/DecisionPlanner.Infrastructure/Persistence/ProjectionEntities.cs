namespace DecisionPlanner.Infrastructure.Persistence;

using DecisionPlanner.Domain.Meeting;

public sealed class TopicProjection
{
    public Guid Id { get; set; }
    public MeetingId MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsGeneral { get; set; }
}

public sealed class ProposalProjection
{
    public Guid Id { get; set; }
    public MeetingId MeetingId { get; set; }
    public Guid TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class DiscussionEntryProjection
{
    public Guid Id { get; set; }
    public MeetingId MeetingId { get; set; }
    public Guid TopicId { get; set; }
    public Guid? ProposalId { get; set; }
    public Guid AuthorParticipantId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
