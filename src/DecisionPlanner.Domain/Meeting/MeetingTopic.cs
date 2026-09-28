namespace DecisionPlanner.Domain.Meeting;

public sealed class MeetingTopic
{
    private readonly List<Proposal> _proposals = [];
    private readonly List<DiscussionEntry> _discussionEntries = [];

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public bool IsGeneral { get; private set; }
    public IReadOnlyCollection<Proposal> Proposals => _proposals.AsReadOnly();
    public IReadOnlyCollection<DiscussionEntry> DiscussionEntries => _discussionEntries.AsReadOnly();

    private MeetingTopic() { }

    internal MeetingTopic(Guid id, string title, bool isGeneral)
    {
        Id = id;
        Title = title;
        IsGeneral = isGeneral;
    }

    internal void AddProposal(Proposal proposal) => _proposals.Add(proposal);
    internal void AddDiscussionEntry(DiscussionEntry entry) => _discussionEntries.Add(entry);
}
