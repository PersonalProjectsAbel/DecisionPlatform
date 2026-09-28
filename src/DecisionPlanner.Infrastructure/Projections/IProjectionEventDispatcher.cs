namespace DecisionPlanner.Infrastructure.Projections;

public interface IProjectionEventDispatcher
{
    Task<bool> DispatchAsync(
        string eventType,
        ReadOnlyMemory<byte> eventData,
        CancellationToken cancellationToken);
}
