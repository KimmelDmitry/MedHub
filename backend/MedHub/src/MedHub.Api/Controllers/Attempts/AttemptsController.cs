using Asp.Versioning;
using MedHub.Application.Attempts.CompleteAttempt;
using MedHub.Application.Attempts.GetActiveAttemptForLesson;
using MedHub.Application.Attempts.GetAttemptResult;
using MedHub.Application.Attempts.StartAttempt;
using MedHub.Application.Attempts.SubmitCheckpointAnswer;
using MedHub.Domain.Abstractions;
using MedHub.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MedHub.Api.Controllers.Attempts;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}")]
public sealed class AttemptsController : ControllerBase
{
    private readonly ISender _sender;

    public AttemptsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("lessons/{lessonId:guid}/attempts/start")]
    [HasPermission(Permissions.AttemptsWrite)]
    public async Task<IActionResult> Start(
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var command = new StartAttemptCommand(lessonId);

        Result<StartAttemptResponse> result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("lessons/{lessonId:guid}/attempts/active")]
    [HasPermission(Permissions.AttemptsRead)]
    public async Task<IActionResult> GetActive(
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var query = new GetActiveAttemptForLessonQuery(lessonId);

        Result<ActiveAttemptResponse> result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("attempts/{attemptId:guid}/answers")]
    [HasPermission(Permissions.AttemptsWrite)]
    public async Task<IActionResult> SubmitAnswer(
        Guid attemptId,
        [FromBody] SubmitCheckpointAnswerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitCheckpointAnswerCommand(
            attemptId,
            request.QuestionId,
            request.SelectedOptionIds,
            request.TextAnswer);

        Result<SubmitCheckpointAnswerResponse> result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("attempts/{attemptId:guid}/complete")]
    [HasPermission(Permissions.AttemptsWrite)]
    public async Task<IActionResult> Complete(
        Guid attemptId,
        CancellationToken cancellationToken)
    {
        var command = new CompleteAttemptCommand(attemptId);

        Result<CompleteAttemptResponse> result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("attempts/{attemptId:guid}/result")]
    [HasPermission(Permissions.AttemptsRead)]
    public async Task<IActionResult> GetResult(
        Guid attemptId,
        CancellationToken cancellationToken)
    {
        var query = new GetAttemptResultQuery(attemptId);

        Result<AttemptResultResponse> result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }
}