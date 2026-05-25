using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Checkpoints.CreateCheckpoint;

internal sealed class CreateCheckpointCommandHandler
    : ICommandHandler<CreateCheckpointCommand, Guid>
{
    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public CreateCheckpointCommandHandler(
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        ICheckpointRepository checkpointRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _videoRepository = videoRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _checkpointRepository = checkpointRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<Guid>> Handle(
        CreateCheckpointCommand command,
        CancellationToken cancellationToken)
    {
        var video = await _videoRepository.GetByIdAsync(
            command.VideoId,
            cancellationToken);

        if (video is null)
        {
            return Result.Failure<Guid>(VideoErrors.NotFound);
        }

        if (video.DurationSeconds is null || video.Status != VideoStatus.Ready)
        {
            return Result.Failure<Guid>(
                new Error(
                    "Checkpoint.VideoNotReady",
                    "Видео должно быть обработано до создания чекпоинтов"));
        }

        var accessResult = await CheckpointAccess.EnsureCanManageVideoAsync(
            video,
            _lessonRepository,
            _courseRepository,
            _userContext,
            cancellationToken);

        if (accessResult.IsFailure)
        {
            return Result.Failure<Guid>(accessResult.Error);
        }

        var checkpointResult = Checkpoint.Create(
            video.Id,
            command.TimestampSeconds,
            command.OrderNumber,
            video.DurationSeconds.Value,
            command.Title,
            command.IsRequired,
            command.IsGraded);

        if (checkpointResult.IsFailure)
        {
            return Result.Failure<Guid>(checkpointResult.Error);
        }

        var checkpoint = checkpointResult.Value;

        _checkpointRepository.Add(checkpoint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(checkpoint.Id);
    }
}
