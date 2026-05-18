using MedHub.Application.Abstractions.Messaging;
using MedHub.Application.Abstractions.Storage;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Media;

namespace MedHub.Application.Media.AbortVideoUpload;

internal sealed class AbortVideoUploadCommandHandler
    : ICommandHandler<AbortVideoUploadCommand>
{
    private readonly IVideoRepository _videoRepository;
    private readonly IVideoStorageProvider _storage;
    private readonly IUnitOfWork _unitOfWork;

    public AbortVideoUploadCommandHandler(
        IVideoRepository videoRepository,
        IVideoStorageProvider storage,
        IUnitOfWork unitOfWork)
    {
        _videoRepository = videoRepository;
        _storage = storage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        AbortVideoUploadCommand command,
        CancellationToken ct)
    {
        VideoMaterial? video = await _videoRepository.GetByIdAsync(command.VideoId, ct);
        if (video is null)
            return Result.Failure(VideoErrors.NotFound);

        // abort можно делать только пока грузим
        if (video.Status != VideoStatus.Uploading)
            return Result.Failure(VideoErrors.InvalidStatusTransition);

        if (video.UploadId is null || video.StorageKey is null)
        {
            return Result.Failure(
                new Error(
                    "Video.UploadNotInitialized",
                    "Загрузка не была инициализирована"));
        }

        // 🔥 отменяем multipart upload в S3/MinIO
        await _storage.AbortMultipartUploadAsync(
            video.StorageKey,
            video.UploadId,
            ct);

        // 🔥 доменная логика
        var result = video.MarkAsFailed("Upload aborted by user");
        if (result.IsFailure)
            return result;

        await _videoRepository.UpdateAsync(video, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}