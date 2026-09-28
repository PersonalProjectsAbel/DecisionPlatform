using DecisionPlanner.Infrastructure.Projections;
using Grpc.Core;
using KurrentDB.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DecisionPlanner.Infrastructure.Projections.Meeting;

public sealed class MeetingProjectionWorker(
    KurrentDBPersistentSubscriptionsClient subscriptionsClient,
    IServiceScopeFactory scopeFactory,
    ILogger<MeetingProjectionWorker> logger) : BackgroundService
{
    private const string SubscriptionGroup = "meeting-postgres-projection-v1";
    private const string MeetingStreamPrefix = "meeting-";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnsureSubscriptionExistsAsync(stoppingToken);
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception,
                    "Meeting projection subscription failed; reconnecting shortly.");
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }

    private async Task EnsureSubscriptionExistsAsync(CancellationToken cancellationToken)
    {
        var settings = new PersistentSubscriptionSettings(startFrom: Position.Start);

        try
        {
            await subscriptionsClient.CreateToAllAsync(
                SubscriptionGroup,
                StreamFilter.Prefix(MeetingStreamPrefix),
                settings,
                cancellationToken: cancellationToken);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.AlreadyExists)
        {
            // The server retains the group's checkpoint across API restarts.
        }
    }

    private async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        await using var subscription = subscriptionsClient.SubscribeToAll(
            SubscriptionGroup,
            cancellationToken: cancellationToken);

        await foreach (var message in subscription.Messages.WithCancellation(cancellationToken))
        {
            if (message is not PersistentSubscriptionMessage.Event(var resolvedEvent, _))
                continue;

            try
            {
                using var scope = scopeFactory.CreateScope();
                var dispatcher = scope.ServiceProvider
                    .GetRequiredService<IProjectionEventDispatcher>();
                var handled = await dispatcher.DispatchAsync(
                    resolvedEvent.OriginalEvent.EventType,
                    resolvedEvent.OriginalEvent.Data,
                    cancellationToken);

                if (!handled)
                {
                    logger.LogWarning(
                        "No Meeting projection handler is registered for event type {EventType}.",
                        resolvedEvent.OriginalEvent.EventType);
                }

                await subscription.Ack(resolvedEvent);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception,
                    "Failed projecting event {EventType}; requesting redelivery.",
                    resolvedEvent.OriginalEvent.EventType);
                await subscription.Nack(
                    PersistentSubscriptionNakEventAction.Retry,
                    exception.Message,
                    resolvedEvent);
            }
        }
    }
}
