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
