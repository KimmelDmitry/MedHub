using Asp.Versioning;
using MedHub.Application.Questions.AddAnswerOption;
using MedHub.Application.Questions.CreateQuestion;
using MedHub.Application.Questions.DeleteQuestion;
using MedHub.Application.Questions.GetCheckpointQuestions;
using MedHub.Application.Questions.GetQuestionById;
using MedHub.Application.Questions.RemoveAnswerOption;
using MedHub.Application.Questions.UpdateAnswerOption;
using MedHub.Application.Questions.UpdateQuestionSettings;
using MedHub.Application.Questions.UpdateQuestionText;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints.Enums;
using MedHub.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MedHub.Api.Controllers.Questions;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}")]
public sealed class QuestionsController : ControllerBase
{
    private readonly ISender _sender;

    public QuestionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("questions/{questionId:guid}")]
    [HasPermission(Permissions.QuestionsRead)]
    public async Task<IActionResult> GetById(
        Guid questionId,
        CancellationToken cancellationToken)
    {
        var query = new GetQuestionByIdQuery(questionId);

        Result<QuestionResponse> result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("checkpoints/{checkpointId:guid}/questions")]
    [HasPermission(Permissions.QuestionsRead)]
    public async Task<IActionResult> GetByCheckpoint(
        Guid checkpointId,
        CancellationToken cancellationToken)
    {
        var query = new GetCheckpointQuestionsQuery(checkpointId);

        Result<IReadOnlyList<CheckpointQuestionResponse>> result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("checkpoints/{checkpointId:guid}/questions")]
    [HasPermission(Permissions.QuestionsCreate)]
    public async Task<IActionResult> Create(
        Guid checkpointId,
        [FromBody] CreateQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateQuestionCommand(
            checkpointId,
            request.Text,
            request.Type,
            request.AllowRetry,
            request.TimeLimitSeconds,
            request.RevealCorrectAnswer,
            request.CorrectTextAnswer);

        Result<Guid> result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPatch("questions/{questionId:guid}/text")]
    [HasPermission(Permissions.QuestionsUpdate)]
    public async Task<IActionResult> UpdateText(
        Guid questionId,
        [FromBody] UpdateQuestionTextRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateQuestionTextCommand(questionId, request.Text);

        Result result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpPatch("questions/{questionId:guid}/settings")]
    [HasPermission(Permissions.QuestionsUpdate)]
    public async Task<IActionResult> UpdateSettings(
        Guid questionId,
        [FromBody] UpdateQuestionSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateQuestionSettingsCommand(
            questionId,
            request.AllowRetry,
            request.TimeLimitSeconds,
            request.RevealCorrectAnswer,
            request.CorrectTextAnswer);

        Result result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpPost("questions/{questionId:guid}/options")]
    [HasPermission(Permissions.QuestionsUpdate)]
    public async Task<IActionResult> AddOption(
        Guid questionId,
        [FromBody] AddAnswerOptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddAnswerOptionCommand(
            questionId,
            request.Text,
            request.IsCorrect);

        Result<Guid> result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPatch("questions/{questionId:guid}/options/{answerOptionId:guid}")]
    [HasPermission(Permissions.QuestionsUpdate)]
    public async Task<IActionResult> UpdateOption(
        Guid questionId,
        Guid answerOptionId,
        [FromBody] UpdateAnswerOptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAnswerOptionCommand(
            questionId,
            answerOptionId,
            request.Text,
            request.IsCorrect);

        Result result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpDelete("questions/{questionId:guid}/options/{answerOptionId:guid}")]
    [HasPermission(Permissions.QuestionsUpdate)]
    public async Task<IActionResult> RemoveOption(
        Guid questionId,
        Guid answerOptionId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveAnswerOptionCommand(questionId, answerOptionId);

        Result result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpDelete("questions/{questionId:guid}")]
    [HasPermission(Permissions.QuestionsDelete)]
    public async Task<IActionResult> Delete(
        Guid questionId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteQuestionCommand(questionId);

        Result result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }
}
