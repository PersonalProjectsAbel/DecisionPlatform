namespace DecisionPlanner.Domain.Meeting.Events;

public abstract record MeetingEvent;

public sealed record MeetingCreatedEvent(
    Guid MeetingId,
    string Title,
    string? Description,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt) : MeetingEvent;

public sealed record ParticipantAddedEvent(
    Guid MeetingId,
    Guid ParticipantId,
    string Name,
    string Email) : MeetingEvent;

public sealed record TopicCreatedEvent(
    Guid MeetingId,
    Guid TopicId,
    string Title,
    bool IsGeneral) : MeetingEvent;

public sealed record ProposalCreatedEvent(
    Guid MeetingId,
    Guid TopicId,
    Guid ProposalId,
    string Title,
    string? Description) : MeetingEvent;

public sealed record DiscussionEntryAddedEvent(
    Guid MeetingId,
    Guid TopicId,
    Guid? ProposalId,
    Guid EntryId,
    Guid AuthorParticipantId,
    string AuthorName,
    string Content,
    DateTimeOffset CreatedAt) : MeetingEvent;

public sealed record MeetingStartedEvent(Guid MeetingId) : MeetingEvent;

public sealed record MeetingCompletedEvent(Guid MeetingId) : MeetingEvent;

public sealed record MeetingCancelledEvent(Guid MeetingId) : MeetingEvent;

public sealed record MeetingMinutesDraftGeneratedEvent(
    Guid MeetingId,
    Guid CompletedEventId,
    string Content,
    DateTimeOffset GeneratedAt) : MeetingEvent;
