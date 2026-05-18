using MedHub.Application.Abstractions.Messaging;
using MedHub.Application.Abstractions.Storage;
using MedHub.Application.Media.Contracts;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Media;

namespace MedHub.Application.Media.StartVideoUpload;

internal sealed class StartVideoUploadCommandHandler
    : ICommandHandler<StartVideoUploadCommand, StartVideoUploadResult>
{
    private const int DefaultChunkSize = 10 * 1024 * 1024; // 10 MB

    private readonly IVideoRepository _videoRepository;
    private readonly IVideoStorageProvider _storage;

    public StartVideoUploadCommandHandler(
        IVideoRepository videoRepository,
        IVideoStorageProvider storage)
    {
        _videoRepository = videoRepository;
        _storage = storage;
    }

    public async Task<Result<StartVideoUploadResult>> Handle(
        StartVideoUploadCommand command,
        CancellationToken ct)
    {
        var title = Path.GetFileNameWithoutExtension(command.FileName);

        var videoResult = VideoMaterial.Create(
            command.LessonId,
            title,
            command.FileName);

        if (videoResult.IsFailure)
            return Result.Failure<StartVideoUploadResult>(videoResult.Error);

        var video = videoResult.Value;

        var objectKey = $"videos/{video.Id:N}/raw/{command.FileName}";

        var init = await _storage.StartMultipartUploadAsync(
            objectKey,
            command.ContentType,
            command.SizeBytes,
            ct);

        video.StartUpload(objectKey, init.UploadId, command.SizeBytes);

        await _videoRepository.AddAsync(video, ct);

        var partsCount = (int)Math.Ceiling((double)command.SizeBytes / DefaultChunkSize);

        IReadOnlyList<ChunkUploadUrlDto> urlsDto = await _storage.GetUploadUrlsAsync(
            objectKey,
            init.UploadId,
            partsCount,
            ct);

        List<ChunkUploadUrl> urls = urlsDto
            .Select(x => new ChunkUploadUrl(
                x.PartNumber,
                x.UploadUrl))
            .ToList();

        return Result.Success(new StartVideoUploadResult(
            video.Id,
            init.UploadId,
            DefaultChunkSize,
            urls));
    }
}