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

    public async Task UpsertTopicAsync(TopicReadModel topic, CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO topics ("Id", "MeetingId", "Title", "IsGeneral")
            VALUES ({topic.Id}, {topic.MeetingId}, {topic.Title}, {topic.IsGeneral})
            ON CONFLICT ("Id") DO UPDATE SET
                "MeetingId" = EXCLUDED."MeetingId",
                "Title" = EXCLUDED."Title",
                "IsGeneral" = EXCLUDED."IsGeneral";
            """, cancellationToken);
    }

    public async Task UpsertProposalAsync(ProposalReadModel proposal, CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO proposals ("Id", "MeetingId", "TopicId", "Title", "Description")
            VALUES ({proposal.Id}, {proposal.MeetingId}, {proposal.TopicId},
                    {proposal.Title}, {proposal.Description})
            ON CONFLICT ("Id") DO UPDATE SET
                "MeetingId" = EXCLUDED."MeetingId",
                "TopicId" = EXCLUDED."TopicId",
                "Title" = EXCLUDED."Title",
                "Description" = EXCLUDED."Description";
            """, cancellationToken);
    }

    public async Task UpsertDiscussionEntryAsync(
        DiscussionEntryReadModel entry,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO discussion_entries
                ("Id", "MeetingId", "TopicId", "ProposalId", "AuthorParticipantId",
                 "AuthorName", "Content", "CreatedAt")
            VALUES
                ({entry.Id}, {entry.MeetingId}, {entry.TopicId}, {entry.ProposalId},
                 {entry.AuthorParticipantId}, {entry.AuthorName}, {entry.Content}, {entry.CreatedAt})
            ON CONFLICT ("Id") DO UPDATE SET
                "MeetingId" = EXCLUDED."MeetingId",
                "TopicId" = EXCLUDED."TopicId",
                "ProposalId" = EXCLUDED."ProposalId",
                "AuthorParticipantId" = EXCLUDED."AuthorParticipantId",
                "AuthorName" = EXCLUDED."AuthorName",
                "Content" = EXCLUDED."Content",
                "CreatedAt" = EXCLUDED."CreatedAt";
            """, cancellationToken);
    }

    public async Task<IReadOnlyList<DiscussionEntryReadModel>> GetDiscussionEntriesAsync(
        Guid meetingId,
        Guid? topicId,
        Guid? proposalId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Database.SqlQuery<DiscussionEntryReadModel>($"""
            SELECT d."Id",
                   d."MeetingId",
                   d."TopicId",
                   t."Title" AS "TopicTitle",
                   d."ProposalId",
                   p."Title" AS "ProposalTitle",
                   d."AuthorParticipantId",
                   d."AuthorName",
                   d."Content",
                   d."CreatedAt"
            FROM discussion_entries AS d
            INNER JOIN topics AS t ON t."Id" = d."TopicId"
            LEFT JOIN proposals AS p ON p."Id" = d."ProposalId"
            WHERE d."MeetingId" = {meetingId}
              AND (CAST({topicId} AS uuid) IS NULL OR d."TopicId" = {topicId})
              AND (CAST({proposalId} AS uuid) IS NULL OR d."ProposalId" = {proposalId})
            ORDER BY d."CreatedAt", d."Id";
            """).ToListAsync(cancellationToken);
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
