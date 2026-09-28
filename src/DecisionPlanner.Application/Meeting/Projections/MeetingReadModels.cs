namespace DecisionPlanner.Application.Meeting.Projections;

public sealed record MeetingReadModel(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Status);

public sealed record ParticipantReadModel(
    Guid Id,
    Guid MeetingId,
    string Name,
    string Email);

public sealed record TopicReadModel(
    Guid Id,
    Guid MeetingId,
    string Title,
    bool IsGeneral);

public sealed record ProposalReadModel(
    Guid Id,
    Guid MeetingId,
    Guid TopicId,
    string Title,
    string? Description);

public sealed record DiscussionEntryReadModel(
    Guid Id,
    Guid MeetingId,
    Guid TopicId,
    string TopicTitle,
    Guid? ProposalId,
    string? ProposalTitle,
    Guid AuthorParticipantId,
    string AuthorName,
    string Content,
    DateTimeOffset CreatedAt);
