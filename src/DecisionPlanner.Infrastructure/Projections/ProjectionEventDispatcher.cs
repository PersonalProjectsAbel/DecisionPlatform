namespace DecisionPlanner.Infrastructure.Projections;

public sealed class ProjectionEventDispatcher(
    IEnumerable<IProjectionEventHandler> handlers) : IProjectionEventDispatcher
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<IProjectionEventHandler>>
        _handlersByEventType = handlers
            .GroupBy(handler => handler.EventType, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<IProjectionEventHandler>)group.ToArray(),
                StringComparer.Ordinal);

    public async Task<bool> DispatchAsync(
        string eventType,
        ReadOnlyMemory<byte> eventData,
        CancellationToken cancellationToken)
    {
        if (!_handlersByEventType.TryGetValue(eventType, out var handlers))
            return false;

        foreach (var handler in handlers)
            await handler.HandleAsync(eventData, cancellationToken);

        return true;
    }
}
