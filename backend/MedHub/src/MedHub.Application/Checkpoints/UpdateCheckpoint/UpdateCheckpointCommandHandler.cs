using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Enums;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Checkpoints.UpdateCheckpoint;

internal sealed class UpdateCheckpointCommandHandler
    : ICommandHandler<UpdateCheckpointCommand>
{
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public UpdateCheckpointCommandHandler(
        ICheckpointRepository checkpointRepository,
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _checkpointRepository = checkpointRepository;
        _videoRepository = videoRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result> Handle(
        UpdateCheckpointCommand command,
        CancellationToken cancellationToken)
    {
        var checkpoint = await _checkpointRepository.GetByIdAsync(
            command.CheckpointId,
            cancellationToken);

        if (checkpoint is null)
        {
            return Result.Failure(CheckpointErrors.NotFound);
        }

        var accessResult = await CheckpointAccess.EnsureCanManageCheckpointAsync(
            checkpoint,
            _videoRepository,
            _lessonRepository,
            _courseRepository,
            _userContext,
            cancellationToken);

        if (accessResult.IsFailure)
        {
            return accessResult;
        }

        if (checkpoint.Status == CheckpointStatus.Archived)
        {
            return Result.Failure(CheckpointErrors.InvalidTransition);
        }

        if (command.Title is not null)
        {
            var titleResult = checkpoint.UpdateTitle(command.Title);
            if (titleResult.IsFailure)
            {
                return titleResult;
            }
        }

        if (command.IsRequired.HasValue || command.IsGraded.HasValue)
        {
            var flagsResult = checkpoint.UpdateFlags(
                command.IsRequired ?? checkpoint.IsRequired,
                command.IsGraded ?? checkpoint.IsGraded);

            if (flagsResult.IsFailure)
            {
                return flagsResult;
            }
        }

        if (command.OrderNumber.HasValue)
        {
            var orderResult = checkpoint.UpdateOrder(command.OrderNumber.Value);
            if (orderResult.IsFailure)
            {
                return orderResult;
            }
        }

        if (command.TimestampSeconds.HasValue)
        {
            var video = await _videoRepository.GetByIdAsync(
                checkpoint.VideoId,
                cancellationToken);

            if (video is null || video.DurationSeconds is null || video.Status != VideoStatus.Ready)
            {
                return Result.Failure(
                    new Error(
                        "Checkpoint.VideoNotReady",
                        "Видео недоступно для обновления контрольной точки"));
            }

            var timestampResult = checkpoint.UpdateTimestamp(
                command.TimestampSeconds.Value,
                video.DurationSeconds.Value);

            if (timestampResult.IsFailure)
            {
                return timestampResult;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
