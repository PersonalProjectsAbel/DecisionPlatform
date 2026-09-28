using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeetingAggregate = DecisionPlanner.Domain.Meeting.Meeting;

namespace DecisionPlanner.Infrastructure.Persistence.Configurations;

public sealed class TopicProjectionConfiguration : IEntityTypeConfiguration<TopicProjection>
{
    public void Configure(EntityTypeBuilder<TopicProjection> builder)
    {
        builder.ToTable("topics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MeetingId)
            .HasConversion(id => id.Value, value => new DecisionPlanner.Domain.Meeting.MeetingId(value));
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.MeetingId);
        builder.HasOne<MeetingAggregate>().WithMany().HasForeignKey(x => x.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
