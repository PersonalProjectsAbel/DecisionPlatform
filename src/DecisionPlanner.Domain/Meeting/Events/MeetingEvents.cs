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

public sealed record MeetingStartedEvent(Guid MeetingId) : MeetingEvent;

public sealed record MeetingCompletedEvent(Guid MeetingId) : MeetingEvent;

public sealed record MeetingCancelledEvent(Guid MeetingId) : MeetingEvent;
