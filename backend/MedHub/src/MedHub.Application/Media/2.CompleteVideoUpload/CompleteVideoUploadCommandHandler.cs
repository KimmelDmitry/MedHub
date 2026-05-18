using MedHub.Application.Abstractions.Messaging;
using MedHub.Application.Abstractions.Storage;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Media;

namespace MedHub.Application.Media.CompleteVideoUpload;

internal sealed class CompleteVideoUploadCommandHandler
    : ICommandHandler<CompleteVideoUploadCommand>
{
    private readonly IVideoRepository _videoRepository;
    private readonly IVideoStorageProvider _storage;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteVideoUploadCommandHandler(
        IVideoRepository videoRepository,
        IVideoStorageProvider storage,
        IUnitOfWork unitOfWork)
    {
        _videoRepository = videoRepository;
        _storage = storage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        CompleteVideoUploadCommand command,
        CancellationToken ct)
    {
        var video = await _videoRepository.GetByIdAsync(command.VideoId, ct);
        if (video is null)
            return Result.Failure(VideoErrors.NotFound);

        if (video.Status != VideoStatus.Uploading)
            return Result.Failure(VideoErrors.InvalidStatusTransition);

        if (video.UploadId is null || video.StorageKey is null)
        {
            return Result.Failure(
                new Error(
                    "Video.UploadNotInitialized",
                    "Загрузка видео не была инициализирована"));
        }

        if (!string.Equals(video.UploadId, command.UploadId, StringComparison.Ordinal))
        {
            return Result.Failure(
                new Error(
                    "Video.UploadIdMismatch",
                    "UploadId не совпадает"));
        }

        await _storage.CompleteMultipartUploadAsync(
            video.StorageKey,
            video.UploadId,
            command.PartETags,
            ct);

        var markResult = video.MarkUploaded();
        if (markResult.IsFailure)
            return markResult;

        await _videoRepository.UpdateAsync(video, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}