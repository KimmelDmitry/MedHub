using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Checkpoints.GetCheckpointById;

internal sealed class GetCheckpointByIdQueryHandler
    : IQueryHandler<GetCheckpointByIdQuery, CheckpointResponse>
{
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserContext _userContext;

    public GetCheckpointByIdQueryHandler(
        ICheckpointRepository checkpointRepository,
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IUserContext userContext)
    {
        _checkpointRepository = checkpointRepository;
        _videoRepository = videoRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _userContext = userContext;
    }

    public async Task<Result<CheckpointResponse>> Handle(
        GetCheckpointByIdQuery query,
        CancellationToken cancellationToken)
    {
        var checkpoint = await _checkpointRepository.GetByIdAsync(
            query.CheckpointId,
            cancellationToken);

        if (checkpoint is null)
        {
            return Result.Failure<CheckpointResponse>(CheckpointErrors.NotFound);
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
            return Result.Failure<CheckpointResponse>(accessResult.Error);
        }

        var response = new CheckpointResponse(
            checkpoint.Id,
            checkpoint.VideoId,
            checkpoint.Timestamp.Value,
            checkpoint.OrderNumber,
            checkpoint.Title,
            checkpoint.IsRequired,
            checkpoint.IsGraded,
            checkpoint.Status.ToString(),
            checkpoint.Questions
                .OrderBy(x => x.Id)
                .Select(x => new CheckpointQuestionResponse(
                    x.Id,
                    x.Text.Value,
                    x.Type.ToString(),
                    x.AllowRetry,
                    x.TimeLimitSeconds,
                    x.RevealCorrectAnswer,
                    x.CorrectTextAnswer,
                    x.AnswerOptions
                        .Select(o => new CheckpointAnswerOptionResponse(
                            o.Id,
                            o.Text.Value,
                            o.IsCorrect))
                        .ToList()))
                .ToList());

        return Result.Success(response);
    }
}
