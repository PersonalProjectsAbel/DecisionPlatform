using System.Text.Json;
using DecisionPlanner.Domain.Meeting.Events;
using Microsoft.Extensions.Logging;

namespace DecisionPlanner.Infrastructure.Projections.Meeting;

public abstract class MeetingProjectionHandlerBase<TEvent>(
    ILogger logger) : IProjectionEventHandler
    where TEvent : MeetingEvent
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public abstract string EventType { get; }

    public async Task HandleAsync(
        ReadOnlyMemory<byte> eventData,
        CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<TEvent>(eventData.Span, JsonOptions)
            ?? throw new InvalidOperationException(
                $"Event payload for '{EventType}' was empty or invalid.");

        await ProjectAsync(@event, cancellationToken);
        logger.LogDebug("Projected event {EventType}", EventType);
    }

    protected abstract Task ProjectAsync(
        TEvent @event,
        CancellationToken cancellationToken);
}
