using DecisionPlanner.Application.Meeting.Projections;
using DecisionPlanner.Application.Meeting;
using DecisionPlanner.Infrastructure.Meeting;
using DecisionPlanner.Infrastructure.Persistence;
using DecisionPlanner.Infrastructure.Persistence.Repositories;
using DecisionPlanner.Infrastructure.Projections;
using DecisionPlanner.Infrastructure.Projections.Meeting;
using KurrentDB.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DecisionPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var eventStoreConnectionString = configuration.GetConnectionString("EventStore")
            ?? throw new InvalidOperationException(
                "Connection string 'EventStore' is not configured.");

        services.AddSingleton(_ => new KurrentDBClient(
            KurrentDBClientSettings.Create(eventStoreConnectionString)));
        services.AddSingleton(_ => new KurrentDBPersistentSubscriptionsClient(
            KurrentDBClientSettings.Create(eventStoreConnectionString)));
        services.AddScoped<IMeetingRepository, MeetingRepository>();
        services.AddScoped<IMeetingReadModelRepository, MeetingReadModelRepository>();
        services.AddScoped<IProjectionEventDispatcher, ProjectionEventDispatcher>();
        services.AddScoped<IProjectionEventHandler, MeetingCreatedProjectionHandler>();
        services.AddScoped<IProjectionEventHandler, ParticipantAddedProjectionHandler>();
        services.AddScoped<IProjectionEventHandler, TopicCreatedProjectionHandler>();
        services.AddScoped<IProjectionEventHandler, ProposalCreatedProjectionHandler>();
        services.AddScoped<IProjectionEventHandler, DiscussionEntryAddedProjectionHandler>();
        services.AddScoped<IProjectionEventHandler, MeetingStartedProjectionHandler>();
        services.AddScoped<IProjectionEventHandler, MeetingCompletedProjectionHandler>();
        services.AddScoped<IProjectionEventHandler, MeetingCancelledProjectionHandler>();
        services.AddHostedService<MeetingProjectionWorker>();

        services.AddDbContext<DecisionPlannerDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("DecisionPlanner.Migrations"));
        });

        return services;
    }
}
