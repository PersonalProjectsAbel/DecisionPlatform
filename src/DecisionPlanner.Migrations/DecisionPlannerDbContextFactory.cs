using DecisionPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DecisionPlanner.Migrations;

public sealed class DecisionPlannerDbContextFactory
    : IDesignTimeDbContextFactory<DecisionPlannerDbContext>
{
    public DecisionPlannerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<DecisionPlannerDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5433;Database=decision_planner;Username=postgres;Password=postgres",
            options =>
            {
                options.MigrationsAssembly(
                    typeof(DecisionPlannerDbContextFactory).Assembly.GetName().Name);
            });

        return new DecisionPlannerDbContext(optionsBuilder.Options);
    }
}