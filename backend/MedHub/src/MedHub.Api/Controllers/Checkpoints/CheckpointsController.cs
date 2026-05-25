using Asp.Versioning;
using MedHub.Application.Checkpoints.ArchiveCheckpoint;
using MedHub.Application.Checkpoints.CreateCheckpoint;
using MedHub.Application.Checkpoints.DeleteCheckpoint;
using MedHub.Application.Checkpoints.GetCheckpointById;
using MedHub.Application.Checkpoints.GetVideoCheckpoints;
using MedHub.Application.Checkpoints.PublishCheckpoint;
using MedHub.Application.Checkpoints.UpdateCheckpoint;
using MedHub.Domain.Abstractions;
using MedHub.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MedHub.Api.Controllers.Checkpoints;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/checkpoints")]
public sealed class CheckpointsController : ControllerBase
{
    private readonly ISender _sender;

    public CheckpointsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{checkpointId:guid}")]
    [HasPermission(Permissions.CheckpointsRead)]
    public async Task<IActionResult> GetById(
        Guid checkpointId,
        CancellationToken cancellationToken)
    {
        var query = new GetCheckpointByIdQuery(checkpointId);

        Result<CheckpointResponse> result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("video/{videoId:guid}")]
    [HasPermission(Permissions.CheckpointsRead)]
    public async Task<IActionResult> GetByVideo(
        Guid videoId,
        CancellationToken cancellationToken)
    {
        var query = new GetVideoCheckpointsQuery(videoId);

        Result<IReadOnlyList<VideoCheckpointResponse>> result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [HasPermission(Permissions.CheckpointsCreate)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCheckpointRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCheckpointCommand(
            request.VideoId,
            request.TimestampSeconds,
            request.OrderNumber,
            request.Title,
            request.IsRequired,
            request.IsGraded);

        Result<Guid> result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPatch("{checkpointId:guid}")]
    [HasPermission(Permissions.CheckpointsUpdate)]
    public async Task<IActionResult> Update(
        Guid checkpointId,
        [FromBody] UpdateCheckpointRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCheckpointCommand(
            checkpointId,
            request.Title,
            request.TimestampSeconds,
            request.OrderNumber,
            request.IsRequired,
            request.IsGraded);

        Result result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return NoContent();
    }

    [HttpPost("{checkpointId:guid}/publish")]
    [HasPermission(Permissions.CheckpointsPublish)]
    public async Task<IActionResult> Publish(
        Guid checkpointId,
        CancellationToken cancellationToken)
    {
        var command = new PublishCheckpointCommand(checkpointId);

        Result result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return NoContent();
    }

    [HttpPost("{checkpointId:guid}/archive")]
    [HasPermission(Permissions.CheckpointsArchive)]
    public async Task<IActionResult> Archive(
        Guid checkpointId,
        CancellationToken cancellationToken)
    {
        var command = new ArchiveCheckpointCommand(checkpointId);

        Result result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return NoContent();
    }

    [HttpDelete("{checkpointId:guid}")]
    [HasPermission(Permissions.CheckpointsDelete)]
    public async Task<IActionResult> Delete(
        Guid checkpointId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCheckpointCommand(checkpointId);

        Result result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error);
        }

        return NoContent();
    }

    private IActionResult HandleFailure(Error error)
    {
        if (error.Code.EndsWith(".Forbidden", StringComparison.Ordinal))
        {
            return Forbid();
        }

        if (error.Code.EndsWith(".NotFound", StringComparison.Ordinal))
        {
            return NotFound(error);
        }

        return BadRequest(error);
    }
}
