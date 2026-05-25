using Asp.Versioning;
using MedHub.Application.Student.Enrollments.GetMyEnrollments;
using MedHub.Domain.Abstractions;
using MedHub.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MedHub.Api.Controllers.Student;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/student/enrollments")]
public sealed class StudentEnrollmentsController : ControllerBase
{
    private readonly ISender _sender;

    public StudentEnrollmentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [HasPermission(Permissions.EnrollmentsRead)]
    public async Task<IActionResult> GetMyEnrollments(
        CancellationToken cancellationToken)
    {
        var query = new GetMyEnrollmentsQuery();

        Result<IReadOnlyList<MyEnrollmentResponse>> result =
            await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
