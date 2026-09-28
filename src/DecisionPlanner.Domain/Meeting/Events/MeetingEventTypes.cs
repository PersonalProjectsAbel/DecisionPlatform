namespace DecisionPlanner.Domain.Meeting.Events;

public static class MeetingEventTypes
{
    public const string Created = "meeting.created.v1";
    public const string ParticipantAdded = "meeting.participant-added.v1";
    public const string TopicCreated = "meeting.topic-created.v1";
    public const string ProposalCreated = "meeting.proposal-created.v1";
    public const string DiscussionEntryAdded = "meeting.discussion-entry-added.v1";
    public const string Started = "meeting.started.v1";
    public const string Completed = "meeting.completed.v1";
    public const string Cancelled = "meeting.cancelled.v1";
    public const string MinutesDraftGenerated = "meeting.minutes-draft-generated.v1";
}
