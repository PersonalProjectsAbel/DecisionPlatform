namespace DecisionPlanner.Domain.Meeting;

public readonly record struct MeetingId(Guid Value)
{
    public static MeetingId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}