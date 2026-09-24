using DecisionPlanner.Domain.Meeting.Events;

namespace DecisionPlanner.Domain.Meeting;

public sealed class Meeting
{
    private readonly List<Participant> _participants = [];
    private readonly List<MeetingEvent> _uncommittedEvents = [];

    public MeetingId Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public MeetingStatus Status { get; private set; }
    public IReadOnlyCollection<Participant> Participants => _participants.AsReadOnly();
    public IReadOnlyCollection<MeetingEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    private Meeting() { }

    public static Meeting Create(
        string title,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        string? description = null)
    {
        var meetingId = MeetingId.New();
        Validate(title, startsAt, endsAt, meetingId);

        var meeting = new Meeting();
        meeting.Raise(new MeetingCreatedEvent(
            meetingId.Value, title, description, startsAt, endsAt));
        return meeting;
    }

    public static Meeting Rehydrate(IEnumerable<MeetingEvent> history)
    {
        var meeting = new Meeting();
        foreach (var @event in history)
            meeting.Apply(@event);
        return meeting;
    }

    public void AddParticipant(Participant participant)
    {
        ArgumentNullException.ThrowIfNull(participant);

        if (Status != MeetingStatus.Scheduled)
            throw new InvalidOperationException(
                "Participants can only be added to scheduled meetings.");

        if (_participants.Any(p => p.Id == participant.Id))
            throw new InvalidOperationException(
                "Participant is already part of the meeting.");

        Raise(new ParticipantAddedEvent(
            Id.Value, participant.Id, participant.Name, participant.Email));
    }

    public void Start()
    {
        if (Status != MeetingStatus.Scheduled)
            throw new InvalidOperationException("Only scheduled meetings can be started.");
        Raise(new MeetingStartedEvent(Id.Value));
    }

    public void Complete()
    {
        if (Status != MeetingStatus.InProgress)
            throw new InvalidOperationException("Only meetings in progress can be completed.");
        Raise(new MeetingCompletedEvent(Id.Value));
    }

    public void Cancel()
    {
        if (Status == MeetingStatus.Completed)
            throw new InvalidOperationException("Completed meetings cannot be cancelled.");
        if (Status == MeetingStatus.Cancelled)
            return;
        Raise(new MeetingCancelledEvent(Id.Value));
    }

    public void MarkEventsCommitted()
    {
        _uncommittedEvents.Clear();
    }

    private void Raise(MeetingEvent @event)
    {
        Apply(@event);
        _uncommittedEvents.Add(@event);
    }

    private void Apply(MeetingEvent @event)
    {
        switch (@event)
        {
            case MeetingCreatedEvent created:
                Validate(created.Title, created.StartsAt, created.EndsAt,
                    new MeetingId(created.MeetingId));
                Id = new MeetingId(created.MeetingId);
                Title = created.Title;
                Description = created.Description;
                StartsAt = created.StartsAt;
                EndsAt = created.EndsAt;
                Status = MeetingStatus.Scheduled;
                break;
            case ParticipantAddedEvent participantAdded:
                EnsureMeetingId(participantAdded.MeetingId);
                _participants.Add(new Participant(
                    participantAdded.ParticipantId,
                    participantAdded.Name,
                    participantAdded.Email));
                break;
            case MeetingStartedEvent started:
                EnsureMeetingId(started.MeetingId);
                Status = MeetingStatus.InProgress;
                break;
            case MeetingCompletedEvent completed:
                EnsureMeetingId(completed.MeetingId);
                Status = MeetingStatus.Completed;
                break;
            case MeetingCancelledEvent cancelled:
                EnsureMeetingId(cancelled.MeetingId);
                Status = MeetingStatus.Cancelled;
                break;
            default:
                throw new InvalidOperationException(
                    $"Unsupported meeting event type: {@event.GetType().Name}");
        }
    }

    private void EnsureMeetingId(Guid meetingId)
    {
        if (Id.Value != meetingId)
            throw new InvalidOperationException("Meeting event belongs to a different stream.");
    }

    private static void Validate(
        string title,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        MeetingId id)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("Meeting ID cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Meeting title is required.", nameof(title));
        if (endsAt <= startsAt)
            throw new ArgumentException(
                "Meeting end time must be after start time.", nameof(endsAt));
    }
}
