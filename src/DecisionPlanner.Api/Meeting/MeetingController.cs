using MediatR;
using DecisionPlanner.Api.Meeting.AddParticipant;
using DecisionPlanner.Application.Meeting.AddParticipant;
using DecisionPlanner.Application.Meeting.CancelMeeting;
using DecisionPlanner.Application.Meeting.CompleteMeeting;
using DecisionPlanner.Application.Meeting.CreateMeeting;
using DecisionPlanner.Application.Meeting.GetMeeting;
using DecisionPlanner.Application.Meeting.StartMeeting;
using DecisionPlanner.Domain.Meeting;
using Microsoft.AspNetCore.Mvc;

namespace DecisionPlanner.Api.Meeting;

[ApiController]
[Route("api/meetings")]
public sealed class MeetingController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateMeetingCommand command,
        CancellationToken cancellationToken)
    {
        var meetingId = await sender.Send(command, cancellationToken);

        return Created($"/api/meetings/{meetingId.Value}", new
        {
            id = meetingId.Value
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var meeting = await sender.Send(
            new GetMeetingQuery(new MeetingId(id)),
            cancellationToken);

        return meeting is null
            ? NotFound()
            : Ok(meeting);
    }

    [HttpPost("{id:guid}/participants")]
    public async Task<IActionResult> AddParticipant(
        Guid id,
        AddParticipantRequest request,
        CancellationToken cancellationToken)
    {
        var added = await sender.Send(
            new AddParticipantCommand(
                new MeetingId(id),
                request.Name,
                request.Email),
            cancellationToken);

        return added ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(
        Guid id,
        CancellationToken cancellationToken)
    {
        var started = await sender.Send(
            new StartMeetingCommand(new MeetingId(id)),
            cancellationToken);

        return started ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var completed = await sender.Send(
            new CompleteMeetingCommand(new MeetingId(id)),
            cancellationToken);

        return completed ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var cancelled = await sender.Send(
            new CancelMeetingCommand(new MeetingId(id)),
            cancellationToken);

        return cancelled ? NoContent() : NotFound();
    }
}
