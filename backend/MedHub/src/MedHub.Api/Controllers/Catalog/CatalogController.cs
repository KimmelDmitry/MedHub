using Asp.Versioning;
using MedHub.Application.Student.Catalog.GetCatalogCourse;
using MedHub.Application.Student.Catalog.GetCatalogCourses;
using MedHub.Application.Student.Enrollments.EnrollInCourse;
using MedHub.Domain.Abstractions;
using MedHub.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MedHub.Api.Controllers.Catalog;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/catalog/courses")]
public sealed class CatalogController : ControllerBase
{
    private readonly ISender _sender;

    public CatalogController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [HasPermission(Permissions.CoursesRead)]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCatalogCoursesQuery(page, pageSize);

        Result<PagedResponse<CatalogCourseListItemResponse>> result =
            await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("{courseId:guid}")]
    [HasPermission(Permissions.CoursesRead)]
    public async Task<IActionResult> GetCourse(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var query = new GetCatalogCourseQuery(courseId);

        Result<CatalogCourseResponse> result =
            await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal))
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("{courseId:guid}/enroll")]
    [HasPermission(Permissions.EnrollmentsCreate)]
    public async Task<IActionResult> Enroll(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var command = new EnrollInCourseCommand(courseId);

        Result<EnrollmentResponse> result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal))
            {
                return NotFound(result.Error);
            }

            if (result.Error.Code.EndsWith(".Forbidden", StringComparison.Ordinal))
            {
                return StatusCode(StatusCodes.Status403Forbidden, result.Error);
            }

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
