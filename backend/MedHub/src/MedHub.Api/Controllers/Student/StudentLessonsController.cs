using Asp.Versioning;
using MedHub.Application.Student.Lessons.GetStudentLessonRuntime;
using MedHub.Domain.Abstractions;
using MedHub.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MedHub.Api.Controllers.Student;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/student/lessons")]
public sealed class StudentLessonsController : ControllerBase
{
    private readonly ISender _sender;

    public StudentLessonsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{lessonId:guid}/runtime")]
    [HasPermission(Permissions.LessonsRead)]
    public async Task<IActionResult> GetRuntime(
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var query = new GetStudentLessonRuntimeQuery(lessonId);

        Result<StudentLessonRuntimeResponse> result =
            await _sender.Send(query, cancellationToken);

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

            if (result.Error.Code.EndsWith(".Required", StringComparison.Ordinal))
            {
                return StatusCode(StatusCodes.Status403Forbidden, result.Error);
            }

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
