using DecisionPlanner.Domain.Meeting.Events;

namespace DecisionPlanner.Domain.Meeting;

public sealed class Meeting
{
    private readonly List<Participant> _participants = [];
    private readonly List<MeetingTopic> _topics = [];
    private readonly List<MeetingEvent> _uncommittedEvents = [];
    private readonly HashSet<Guid> _minutesDraftCompletionEventIds = [];
    private readonly List<MeetingMinutesDraftGeneratedEvent> _minutesDrafts = [];

    public MeetingId Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public MeetingStatus Status { get; private set; }
    public IReadOnlyCollection<Participant> Participants => _participants.AsReadOnly();
    public IReadOnlyCollection<MeetingTopic> Topics => _topics.AsReadOnly();
    public IReadOnlyCollection<MeetingEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();
    public IReadOnlyCollection<MeetingMinutesDraftGeneratedEvent> MinutesDrafts =>
        _minutesDrafts.AsReadOnly();

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
        meeting.CreateTopic("General", isGeneral: true);
        return meeting;
    }

    public static Meeting Rehydrate(IEnumerable<MeetingEvent> history)
    {
        var meeting = new Meeting();
        foreach (var @event in history)
            meeting.Apply(@event);
        // Older Meeting streams predate the General topic. Materialize it with a
        // stable ID; the next command persists this backfill as a normal event.
        if (!meeting._topics.Any(topic => topic.IsGeneral))
            meeting.CreateTopic("General", isGeneral: true);
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

    public Guid CreateTopic(string title, bool isGeneral = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Topic title is required.", nameof(title));
        if (title.Trim().Length > 200)
            throw new ArgumentException("Topic title cannot exceed 200 characters.", nameof(title));
        if (isGeneral && _topics.Any(topic => topic.IsGeneral))
            throw new InvalidOperationException("The meeting already has a General topic.");
        if (!isGeneral && _topics.Any(topic =>
                string.Equals(topic.Title, title.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("A topic with this title already exists.");

        var topicId = isGeneral ? Id.Value : Guid.NewGuid();
        Raise(new TopicCreatedEvent(Id.Value, topicId, title.Trim(), isGeneral));
        return topicId;
    }

    public Guid CreateProposal(Guid topicId, string title, string? description)
    {
        var topic = FindTopic(topicId);
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Proposal title is required.", nameof(title));
        if (title.Trim().Length > 200)
            throw new ArgumentException("Proposal title cannot exceed 200 characters.", nameof(title));
        if (description?.Length > 2000)
            throw new ArgumentException("Proposal description cannot exceed 2000 characters.", nameof(description));

        var proposalId = Guid.NewGuid();
        Raise(new ProposalCreatedEvent(
            Id.Value, topicId, proposalId, title.Trim(), description?.Trim()));
        return proposalId;
    }

    public Guid AddDiscussionEntry(
        Guid topicId,
        Guid? proposalId,
        Guid authorParticipantId,
        string content,
        DateTimeOffset? createdAt = null)
    {
        var topic = FindTopic(topicId);
        var author = _participants.FirstOrDefault(participant => participant.Id == authorParticipantId);
        if (author is null)
            throw new InvalidOperationException(
                "Discussion author must be a participant in this meeting.");
        if (proposalId.HasValue && !topic.Proposals.Any(proposal => proposal.Id == proposalId))
            throw new InvalidOperationException("Proposal does not belong to this topic.");
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Discussion content is required.", nameof(content));
        if (content.Trim().Length > 4000)
            throw new ArgumentException("Discussion content cannot exceed 4000 characters.", nameof(content));

        var entryId = Guid.NewGuid();
        Raise(new DiscussionEntryAddedEvent(
            Id.Value, topicId, proposalId, entryId, authorParticipantId, author.Name,
            content.Trim(), createdAt ?? DateTimeOffset.UtcNow));
        return entryId;
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

    public bool RecordMinutesDraft(
        Guid completedEventId,
        string content,
        DateTimeOffset generatedAt)
    {
        if (Status != MeetingStatus.Completed)
            throw new InvalidOperationException(
                "Minutes drafts can only be recorded for completed meetings.");
        if (completedEventId == Guid.Empty)
            throw new ArgumentException("Completion event ID is required.", nameof(completedEventId));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Minutes draft content is required.", nameof(content));
        if (content.Length > 200_000)
            throw new ArgumentException("Minutes draft content is too long.", nameof(content));

        if (_minutesDraftCompletionEventIds.Contains(completedEventId))
            return false;

        Raise(new MeetingMinutesDraftGeneratedEvent(
            Id.Value, completedEventId, content, generatedAt));
        return true;
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
            case TopicCreatedEvent topicCreated:
                EnsureMeetingId(topicCreated.MeetingId);
                if (_topics.Any(topic => topic.Id == topicCreated.TopicId))
                    throw new InvalidOperationException("Topic already exists.");
                _topics.Add(new MeetingTopic(
                    topicCreated.TopicId, topicCreated.Title, topicCreated.IsGeneral));
                break;
            case ProposalCreatedEvent proposalCreated:
                EnsureMeetingId(proposalCreated.MeetingId);
                FindTopic(proposalCreated.TopicId).AddProposal(new Proposal(
                    proposalCreated.ProposalId,
                    proposalCreated.Title,
                    proposalCreated.Description));
                break;
            case DiscussionEntryAddedEvent discussionAdded:
                EnsureMeetingId(discussionAdded.MeetingId);
                FindTopic(discussionAdded.TopicId).AddDiscussionEntry(new DiscussionEntry(
                    discussionAdded.EntryId,
                    discussionAdded.ProposalId,
                    discussionAdded.AuthorParticipantId,
                    discussionAdded.Content,
                    discussionAdded.CreatedAt));
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
            case MeetingMinutesDraftGeneratedEvent draftGenerated:
                EnsureMeetingId(draftGenerated.MeetingId);
                _minutesDraftCompletionEventIds.Add(draftGenerated.CompletedEventId);
                _minutesDrafts.Add(draftGenerated);
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

    private MeetingTopic FindTopic(Guid topicId) =>
        _topics.FirstOrDefault(topic => topic.Id == topicId)
        ?? throw new InvalidOperationException("Topic does not belong to this meeting.");

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
