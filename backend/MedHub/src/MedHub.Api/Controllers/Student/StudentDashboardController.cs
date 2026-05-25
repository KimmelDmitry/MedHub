using Asp.Versioning;
using MedHub.Application.Student.Dashboard.GetStudentDashboard;
using MedHub.Domain.Abstractions;
using MedHub.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MedHub.Api.Controllers.Student;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/student/dashboard")]
public sealed class StudentDashboardController : ControllerBase
{
    private readonly ISender _sender;

    public StudentDashboardController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [HasPermission(Permissions.EnrollmentsRead)]
    public async Task<IActionResult> GetDashboard(
        CancellationToken cancellationToken)
    {
        Result<StudentDashboardResponse> result =
            await _sender.Send(new GetStudentDashboardQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
