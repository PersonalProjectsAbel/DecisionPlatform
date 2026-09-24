namespace DecisionPlanner.Domain.Meeting;

public sealed class Participant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    private Participant()
    {
        // EF Core
    }

    public Participant(Guid id, string name, string email)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Participant ID cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Participant name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Participant email is required.", nameof(email));

        Id = id;
        Name = name;
        Email = email;
    }
}