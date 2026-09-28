using DecisionPlanner.Application.Meeting.Projections;
using DecisionPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DecisionPlanner.Infrastructure.Persistence.Repositories;

public sealed class MeetingReadModelRepository(
    DecisionPlannerDbContext dbContext) : IMeetingReadModelRepository
{
    public async Task UpsertMeetingAsync(
        MeetingReadModel meeting,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO meetings
                ("Id", "Title", "Description", "StartsAt", "EndsAt", "Status")
            VALUES
                ({meeting.Id}, {meeting.Title}, {meeting.Description},
                 {meeting.StartsAt}, {meeting.EndsAt}, {meeting.Status})
            ON CONFLICT ("Id") DO UPDATE SET
                "Title" = EXCLUDED."Title",
                "Description" = EXCLUDED."Description",
                "StartsAt" = EXCLUDED."StartsAt",
                "EndsAt" = EXCLUDED."EndsAt",
                "Status" = EXCLUDED."Status";
            """, cancellationToken);
    }

    public async Task UpsertParticipantAsync(
        ParticipantReadModel participant,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO meeting_participants
                ("Id", "Name", "Email", "MeetingId")
            VALUES
                ({participant.Id}, {participant.Name}, {participant.Email},
                 {participant.MeetingId})
            ON CONFLICT ("Id") DO UPDATE SET
                "Name" = EXCLUDED."Name",
                "Email" = EXCLUDED."Email",
                "MeetingId" = EXCLUDED."MeetingId";
            """, cancellationToken);
    }

    public async Task UpdateMeetingStatusAsync(
        Guid meetingId,
        string status,
        CancellationToken cancellationToken)
    {
        var rowsUpdated = await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE meetings
            SET "Status" = {status}
            WHERE "Id" = {meetingId};
            """, cancellationToken);

        if (rowsUpdated != 1)
            throw new InvalidOperationException(
                $"Meeting projection row '{meetingId}' was not found for status update.");
    }
}
