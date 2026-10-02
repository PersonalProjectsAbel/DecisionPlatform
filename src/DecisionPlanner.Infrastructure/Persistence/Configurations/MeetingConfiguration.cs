using DecisionPlanner.Domain.Meeting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeetingAggregate = DecisionPlanner.Domain.Meeting.Meeting;

namespace DecisionPlanner.Infrastructure.Persistence.Configurations;

public sealed class MeetingConfiguration : IEntityTypeConfiguration<MeetingAggregate>
{
    public void Configure(EntityTypeBuilder<MeetingAggregate> builder)
    {
        builder.ToTable("meetings");

        builder.HasKey(x => x.Id);

        // These collections are represented by KurrentDB event streams, not EF navigations.
        builder.Ignore(x => x.UncommittedEvents);
        builder.Ignore(x => x.MinutesDrafts);
        builder.Ignore(x => x.Topics);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new MeetingId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.StartsAt)
            .IsRequired();

        builder.Property(x => x.EndsAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Navigation(x => x.Participants)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.Participants, participant =>
        {
            participant.ToTable("meeting_participants");

            participant.WithOwner()
                .HasForeignKey("MeetingId");

            participant.HasKey(x => x.Id);

            participant.Property(x => x.Id)
                .ValueGeneratedNever();

            participant.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            participant.Property(x => x.Email)
                .HasMaxLength(320)
                .IsRequired();
        });
    }
}
