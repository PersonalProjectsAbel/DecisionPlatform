using System.Collections.Concurrent;
using System.Text.Json;
using DecisionPlanner.Application.Meeting;
using DecisionPlanner.Domain.Meeting;
using DecisionPlanner.Domain.Meeting.Events;
using KurrentDB.Client;
using MeetingAggregate = DecisionPlanner.Domain.Meeting.Meeting;

namespace DecisionPlanner.Infrastructure.Meeting;

public sealed class MeetingRepository(KurrentDBClient client) : IMeetingRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<MeetingId, ulong> _streamRevisions = new();

    public async Task<MeetingAggregate?> GetByIdAsync(
        MeetingId id,
        CancellationToken cancellationToken)
    {
        var streamName = StreamName(id);
        var result = client.ReadStreamAsync(
            Direction.Forwards,
            streamName,
            StreamPosition.Start,
            cancellationToken: cancellationToken);

        if (await result.ReadState == ReadState.StreamNotFound)
            return null;

        var history = new List<MeetingEvent>();
        ulong? lastRevision = null;
        await foreach (var resolvedEvent in result.WithCancellation(cancellationToken))
        {
            history.Add(Deserialize(resolvedEvent));
            lastRevision = resolvedEvent.OriginalEventNumber.ToUInt64();
        }

        if (history.Count == 0)
            return null;

        _streamRevisions[id] = lastRevision!.Value;
        return MeetingAggregate.Rehydrate(history);
    }

    public async Task SaveChangesAsync(
        MeetingAggregate meeting,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(meeting);
        var events = meeting.UncommittedEvents
            .Select(ToEventData)
            .ToArray();
        if (events.Length == 0)
            return;

        var streamName = StreamName(meeting.Id);
        if (_streamRevisions.TryGetValue(meeting.Id, out var revision))
        {
            await client.AppendToStreamAsync(
                streamName, revision, events, cancellationToken: cancellationToken);
            _streamRevisions[meeting.Id] = revision + (ulong)events.Length;
        }
        else
        {
            await client.AppendToStreamAsync(
                streamName, StreamState.NoStream, events,
                cancellationToken: cancellationToken);
            _streamRevisions[meeting.Id] = (ulong)events.Length - 1;
        }

        meeting.MarkEventsCommitted();
    }

    private static EventData ToEventData(MeetingEvent @event)
    {
        var eventType = @event switch
        {
            MeetingCreatedEvent => MeetingEventTypes.Created,
            ParticipantAddedEvent => MeetingEventTypes.ParticipantAdded,
            MeetingStartedEvent => MeetingEventTypes.Started,
            MeetingCompletedEvent => MeetingEventTypes.Completed,
            MeetingCancelledEvent => MeetingEventTypes.Cancelled,
            _ => throw new InvalidOperationException(
                $"Unsupported meeting event type: {@event.GetType().Name}")
        };

        return new EventData(
            Uuid.NewUuid(), eventType,
            JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), JsonOptions));
    }

    private static MeetingEvent Deserialize(ResolvedEvent resolvedEvent)
    {
        var @event = resolvedEvent.OriginalEvent;
        return @event.EventType switch
        {
            MeetingEventTypes.Created => JsonSerializer.Deserialize<MeetingCreatedEvent>(@event.Data.Span, JsonOptions)!,
            MeetingEventTypes.ParticipantAdded => JsonSerializer.Deserialize<ParticipantAddedEvent>(@event.Data.Span, JsonOptions)!,
            MeetingEventTypes.Started => JsonSerializer.Deserialize<MeetingStartedEvent>(@event.Data.Span, JsonOptions)!,
            MeetingEventTypes.Completed => JsonSerializer.Deserialize<MeetingCompletedEvent>(@event.Data.Span, JsonOptions)!,
            MeetingEventTypes.Cancelled => JsonSerializer.Deserialize<MeetingCancelledEvent>(@event.Data.Span, JsonOptions)!,
            _ => throw new InvalidOperationException($"Unknown meeting event type '{@event.EventType}'.")
        };
    }

    private static string StreamName(MeetingId id) => $"meeting-{id.Value:N}";
}
