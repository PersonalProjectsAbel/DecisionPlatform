using DecisionPlanner.Application.Meeting;
using DecisionPlanner.Infrastructure.Meeting;
using DecisionPlanner.Infrastructure.Persistence;
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
        services.AddScoped<IMeetingRepository, MeetingRepository>();

        services.AddDbContext<DecisionPlannerDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("DecisionPlanner.Migrations"));
        });

        return services;
    }
}
