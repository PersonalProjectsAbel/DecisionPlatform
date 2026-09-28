namespace DecisionPlanner.Domain.Meeting;

public sealed class Proposal
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private Proposal() { }

    internal Proposal(Guid id, string title, string? description)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Proposal ID cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Proposal title is required.", nameof(title));

        Id = id;
        Title = title;
        Description = description;
    }
}
