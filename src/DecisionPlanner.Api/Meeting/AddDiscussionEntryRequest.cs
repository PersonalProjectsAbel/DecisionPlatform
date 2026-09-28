namespace DecisionPlanner.Api.Meeting;

public sealed record AddDiscussionEntryRequest(Guid AuthorParticipantId, string Content);
