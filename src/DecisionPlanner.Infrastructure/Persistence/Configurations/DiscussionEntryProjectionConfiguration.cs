using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeetingAggregate = DecisionPlanner.Domain.Meeting.Meeting;

namespace DecisionPlanner.Infrastructure.Persistence.Configurations;

public sealed class DiscussionEntryProjectionConfiguration
    : IEntityTypeConfiguration<DiscussionEntryProjection>
{
    public void Configure(EntityTypeBuilder<DiscussionEntryProjection> builder)
    {
        builder.ToTable("discussion_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MeetingId)
            .HasConversion(id => id.Value, value => new DecisionPlanner.Domain.Meeting.MeetingId(value));
        builder.Property(x => x.AuthorName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(4000).IsRequired();
        builder.HasIndex(x => new { x.MeetingId, x.CreatedAt });
        builder.HasIndex(x => new { x.TopicId, x.CreatedAt });
        builder.HasIndex(x => new { x.ProposalId, x.CreatedAt });
        builder.HasIndex(x => x.AuthorParticipantId);
        builder.HasOne<MeetingAggregate>().WithMany().HasForeignKey(x => x.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<TopicProjection>().WithMany().HasForeignKey(x => x.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ProposalProjection>().WithMany().HasForeignKey(x => x.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
