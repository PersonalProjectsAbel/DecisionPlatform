using System.Security.Cryptography;
using System.Text;
using DecisionPlanner.Application.Meeting.SubmitMinutesDraft;
using DecisionPlanner.Domain.Meeting;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DecisionPlanner.Api.Meeting;

[ApiController]
[Route("api/internal/meetings/{meetingId:guid}/minutes-draft")]
public sealed class MinutesDraftsController(
    ISender sender,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(
        Guid meetingId,
        SubmitMinutesDraftRequest request,
        [FromHeader(Name = "X-Minutes-Service-Key")] string? serviceKey,
        CancellationToken cancellationToken)
    {
        if (request.CompletedEventId == Guid.Empty || request.GeneratedAt == default)
            return BadRequest("A valid completion event ID and generation time are required.");

        var configuredKey = configuration["MinutesService:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredKey))
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                "Minutes service credentials are not configured.");

        var suppliedBytes = Encoding.UTF8.GetBytes(serviceKey ?? string.Empty);
        var configuredBytes = Encoding.UTF8.GetBytes(configuredKey);
        if (!CryptographicOperations.FixedTimeEquals(suppliedBytes, configuredBytes))
            return Unauthorized();

        var result = await sender.Send(
            new SubmitMinutesDraftCommand(
                new MeetingId(meetingId),
                request.CompletedEventId,
                request.Content,
                request.GeneratedAt),
            cancellationToken);

        return result switch
        {
            SubmitMinutesDraftResult.Stored => NoContent(),
            SubmitMinutesDraftResult.AlreadyStored => NoContent(),
            SubmitMinutesDraftResult.MeetingNotFound => NotFound(),
            SubmitMinutesDraftResult.MeetingNotCompleted => Conflict(
                "Minutes can only be submitted for completed meetings."),
            _ => Problem("Unexpected minutes draft submission result.")
        };
    }
}
