using DecisionPlanner.Domain.Meeting;
using Microsoft.EntityFrameworkCore;
using MeetingAggregate = DecisionPlanner.Domain.Meeting.Meeting;

namespace DecisionPlanner.Infrastructure.Persistence;

public class DecisionPlannerDbContext : DbContext
{
    public DecisionPlannerDbContext(
        DbContextOptions<DecisionPlannerDbContext> options)
        : base(options)
    {
    }

    public DbSet<MeetingAggregate> Meetings => Set<MeetingAggregate>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(DecisionPlannerDbContext).Assembly);
    }
}