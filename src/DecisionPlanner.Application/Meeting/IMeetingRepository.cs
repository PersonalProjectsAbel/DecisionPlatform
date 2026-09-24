using DecisionPlanner.Domain.Meeting;
using MeetingAggregate = DecisionPlanner.Domain.Meeting.Meeting;

namespace DecisionPlanner.Application.Meeting;

public interface IMeetingRepository
{
    Task<MeetingAggregate?> GetByIdAsync(
        MeetingId id,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        MeetingAggregate meeting,
        CancellationToken cancellationToken);
}
