using Asp.Versioning;
using MedHub.Application.Courses.ArchiveCourse;
using MedHub.Application.Courses.CreateCourse;
using MedHub.Application.Courses.GetCourseById;
using MedHub.Application.Courses.PublishCourse;
using MedHub.Application.Courses.UpdateCourseDescription;
using MedHub.Application.Courses.UpdateCourseTitle;
using MedHub.Application.Lessons.GetLessonsByCourse;
using MedHub.Domain.Abstractions;
using MedHub.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MedHub.Api.Controllers.Courses;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/courses")]
public sealed class CoursesController : ControllerBase
{
    private readonly ISender _sender;

    public CoursesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Получить курс по Id
    /// </summary>
    [HttpGet("{courseId:guid}")]
    [HasPermission(Permissions.CoursesRead)]
    public async Task<IActionResult> GetById(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var query = new GetCourseByIdQuery(courseId);

        Result<CourseResponse> result =
            await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }
    
    /// <summary>
    /// Получить структуру курса
    /// </summary>
    [HttpGet("{courseId:guid}/content")]
    [HasPermission(Permissions.CoursesRead)]
    public async Task<IActionResult> GetContent(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var query = new GetLessonsByCourseQuery(courseId);

        Result<IReadOnlyList<LessonResponse>> result =
            await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Создать курс
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.CoursesCreate)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCourseCommand(
            request.Title,
            request.Description);

        Result<Guid> result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Опубликовать курс
    /// </summary>
    [HttpPost("{courseId:guid}/publish")]
    [HasPermission(Permissions.CoursesPublish)]
    public async Task<IActionResult> Publish(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var command = new PublishCourseCommand(courseId);

        Result result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Архивировать курс
    /// </summary>
    [HttpPost("{courseId:guid}/archive")]
    [HasPermission(Permissions.CoursesArchive)]
    public async Task<IActionResult> Archive(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var command = new ArchiveCourseCommand(courseId);

        Result result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Обновить title курса
    /// </summary>
    [HttpPatch("{courseId:guid}/title")]
    [HasPermission(Permissions.CoursesUpdate)]
    public async Task<IActionResult> UpdateTitle(
        Guid courseId,
        [FromBody] UpdateCourseTitleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCourseTitleCommand(
            courseId,
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
    /// Обновить description курса
    /// </summary>
    [HttpPatch("{courseId:guid}/description")]
    [HasPermission(Permissions.CoursesUpdate)]
    public async Task<IActionResult> UpdateDescription(
        Guid courseId,
        [FromBody] UpdateCourseDescriptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCourseDescriptionCommand(
            courseId,
            request.Description);

        Result result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }
}