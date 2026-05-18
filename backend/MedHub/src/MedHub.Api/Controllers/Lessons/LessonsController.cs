using Asp.Versioning;
using MedHub.Application.Lessons.ArchiveLesson;
using MedHub.Application.Lessons.AttachVideoToLesson;
using MedHub.Application.Lessons.CreateLesson;
using MedHub.Application.Lessons.GetLessonById;
using MedHub.Application.Lessons.GetLessonsByCourse;
using MedHub.Application.Lessons.PublishLesson;
using MedHub.Application.Lessons.UpdateLessonContent;
using MedHub.Application.Lessons.UpdateLessonOrder;
using MedHub.Application.Lessons.UpdateLessonTitle;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Lessons;
using MedHub.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using LessonResponse = MedHub.Application.Lessons.LessonResponse;

namespace MedHub.Api.Controllers.Lessons;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/lessons")]
public sealed class LessonsController : ControllerBase
{
    private readonly ISender _sender;

    public LessonsController(ISender sender)
    {
        _sender = sender;
    }
    
    

    /// <summary>
    /// Получить урок по Id
    /// </summary>
    [HttpGet("{lessonId:guid}")]
    [HasPermission(Permissions.LessonsRead)]
    public async Task<IActionResult> GetById(
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var query = new GetLessonByIdQuery(lessonId);

        Result<LessonResponse> result =
            await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Создать урок
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.LessonsCreate)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLessonRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLessonCommand(
            request.CourseId,
            request.Title,
            request.Order,
            request.ContentType,
            request.ContentUrl);

        Result<Guid> result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Опубликовать урок
    /// </summary>
    [HttpPost("{lessonId:guid}/publish")]
    [HasPermission(Permissions.LessonsPublish)]
    public async Task<IActionResult> Publish(
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var command = new PublishLessonCommand(lessonId);

        Result result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Архивировать урок
    /// </summary>
    [HttpPost("{lessonId:guid}/archive")]
    [HasPermission(Permissions.LessonsArchive)]
    public async Task<IActionResult> Archive(
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var command = new ArchiveLessonCommand(lessonId);

        Result result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Обновить title урока
    /// </summary>
    [HttpPatch("{lessonId:guid}/title")]
    [HasPermission(Permissions.LessonsUpdate)]
    public async Task<IActionResult> UpdateTitle(
        Guid lessonId,
        [FromBody] UpdateLessonTitleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLessonTitleCommand(
            lessonId,
            request.Title);

        Result result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Обновить порядок урока
    /// </summary>
    [HttpPatch("{lessonId:guid}/order")]
    [HasPermission(Permissions.LessonsUpdate)]
    public async Task<IActionResult> UpdateOrder(
        Guid lessonId,
        [FromBody] UpdateLessonOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLessonOrderCommand(
            lessonId,
            request.Order);

        Result result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Обновить контент урока
    /// </summary>
    [HttpPatch("{lessonId:guid}/content")]
    [HasPermission(Permissions.LessonsUpdate)]
    public async Task<IActionResult> UpdateContent(
        Guid lessonId,
        [FromBody] UpdateLessonContentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLessonContentCommand(
            lessonId,
            request.ContentUrl,
            request.ContentType);

        Result result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Прикрепить видео к уроку
    /// </summary>
    [HttpPost("{lessonId:guid}/attach-video")]
    [HasPermission(Permissions.LessonsUpdate)]
    public async Task<IActionResult> AttachVideo(
        Guid lessonId,
        [FromBody] AttachVideoToLessonRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AttachVideoToLessonCommand(
            lessonId,
            request.VideoId);

        Result result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }
}
