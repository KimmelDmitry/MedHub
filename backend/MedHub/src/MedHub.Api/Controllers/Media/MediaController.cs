using Asp.Versioning;
using MedHub.Application.Media.AbortVideoUpload;
using MedHub.Application.Media.CompleteVideoUpload;
using MedHub.Application.Media.GetVideoHlsFile;
using MedHub.Application.Media.GetVideoPlayback;
using MedHub.Application.Media.GetVideoStatus;
using MedHub.Application.Media.StartVideoUpload;
using MedHub.Domain.Abstractions;
using MedHub.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MedHub.Api.Controllers.Media;

[ApiController]
[ApiVersion(ApiVersions.V1)]
[Route("api/v{version:apiVersion}/media")]
public sealed class MediaController : ControllerBase
{
    private readonly ISender _sender;

    public MediaController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Инициализация multipart upload
    /// </summary>
    [HttpPost("videos/start-upload")]
    [HasPermission(Permissions.MediaUpload)]
    public async Task<IActionResult> StartUpload(
        [FromBody] StartVideoUploadRequest request,
        CancellationToken cancellationToken)
    {
        var command = new StartVideoUploadCommand(
            request.LessonId,
            request.FileName,
            request.ContentType,
            request.SizeBytes);

        Result<StartVideoUploadResult> result =
            await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Video.Forbidden" ||
                result.Error.Code.EndsWith(".Required", StringComparison.Ordinal))
            {
                return StatusCode(StatusCodes.Status403Forbidden, result.Error);
            }

            if (result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal))
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Завершение multipart upload
    /// </summary>
    [HttpPost("videos/{videoId:guid}/complete-upload")]
    [HasPermission(Permissions.MediaUpload)]
    public async Task<IActionResult> CompleteUpload(
        Guid videoId,
        [FromBody] CompleteVideoUploadRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompleteVideoUploadCommand(
            videoId,
            request.UploadId,
            request.PartETags);

        Result result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Отмена multipart upload
    /// </summary>
    [HttpPost("videos/{videoId:guid}/abort-upload")]
    [HasPermission(Permissions.MediaUpload)]
    public async Task<IActionResult> AbortUpload(
        Guid videoId,
        CancellationToken cancellationToken)
    {
        var command = new AbortVideoUploadCommand(videoId);

        Result result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    /// <summary>
    /// Получить статус обработки видео
    /// </summary>
    [HttpGet("videos/{videoId:guid}/status")]
    [HasPermission(Permissions.MediaRead)]
    public async Task<IActionResult> GetStatus(
        Guid videoId,
        CancellationToken cancellationToken)
    {
        var query = new GetVideoStatusQuery(videoId);

        Result<VideoStatusResponse> result =
            await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Получить playback url и metadata
    /// </summary>
    [HttpGet("videos/{videoId:guid}/playback")]
    [HasPermission(Permissions.MediaRead)]
    public async Task<IActionResult> GetPlayback(
        Guid videoId,
        CancellationToken cancellationToken)
    {
        var query = new GetVideoPlaybackQuery(videoId);

        Result<VideoPlaybackResponse> result =
            await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Video.Forbidden" ||
                result.Error.Code.EndsWith(".Required", StringComparison.Ordinal))
            {
                return StatusCode(StatusCodes.Status403Forbidden, result.Error);
            }

            if (result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal))
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Получить HLS playlist или segment через защищенный API proxy
    /// </summary>
    [HttpGet("videos/{videoId:guid}/hls/{fileName}")]
    [HasPermission(Permissions.MediaRead)]
    public async Task<IActionResult> GetHlsFile(
        Guid videoId,
        string fileName,
        CancellationToken cancellationToken)
    {
        var query = new GetVideoHlsFileQuery(videoId, fileName);

        Result<VideoHlsFileResponse> result =
            await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "Video.Forbidden" ||
                result.Error.Code.EndsWith(".Required", StringComparison.Ordinal))
            {
                return StatusCode(StatusCodes.Status403Forbidden, result.Error);
            }

            if (result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal))
            {
                return NotFound(result.Error);
            }

            return BadRequest(result.Error);
        }

        if (result.Value.ContentLength.HasValue)
        {
            Response.ContentLength = result.Value.ContentLength.Value;
        }

        return new FileStreamResult(result.Value.Content, result.Value.ContentType)
        {
            EnableRangeProcessing = true
        };
    }
}
