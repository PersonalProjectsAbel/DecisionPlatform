namespace DecisionPlanner.Infrastructure.Projections;

public interface IProjectionEventHandler
{
    string EventType { get; }

    Task HandleAsync(
        ReadOnlyMemory<byte> eventData,
        CancellationToken cancellationToken);
}
