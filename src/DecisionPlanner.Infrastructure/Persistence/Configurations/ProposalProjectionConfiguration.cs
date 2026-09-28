using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeetingAggregate = DecisionPlanner.Domain.Meeting.Meeting;

namespace DecisionPlanner.Infrastructure.Persistence.Configurations;

public sealed class ProposalProjectionConfiguration : IEntityTypeConfiguration<ProposalProjection>
{
    public void Configure(EntityTypeBuilder<ProposalProjection> builder)
    {
        builder.ToTable("proposals");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MeetingId)
            .HasConversion(id => id.Value, value => new DecisionPlanner.Domain.Meeting.MeetingId(value));
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.HasIndex(x => x.MeetingId);
        builder.HasIndex(x => x.TopicId);
        builder.HasOne<MeetingAggregate>().WithMany().HasForeignKey(x => x.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<TopicProjection>().WithMany().HasForeignKey(x => x.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
