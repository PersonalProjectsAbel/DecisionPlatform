using System.ComponentModel.DataAnnotations;

namespace DecisionPlanner.Api.Meeting;

public sealed class SubmitMinutesDraftRequest
{
    [Required]
    public Guid CompletedEventId { get; init; }

    [Required]
    [StringLength(200_000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;

    public DateTimeOffset GeneratedAt { get; init; }
}
