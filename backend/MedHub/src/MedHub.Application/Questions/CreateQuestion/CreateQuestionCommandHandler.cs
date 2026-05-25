using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Questions.CreateQuestion;

internal sealed class CreateQuestionCommandHandler
    : ICommandHandler<CreateQuestionCommand, Guid>
{
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateQuestionCommandHandler(
        ICheckpointRepository checkpointRepository,
        IQuestionRepository questionRepository,
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _checkpointRepository = checkpointRepository;
        _questionRepository = questionRepository;
        _videoRepository = videoRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var checkpoint = await _checkpointRepository.GetByIdAsync(
            command.CheckpointId,
            cancellationToken);

        if (checkpoint is null)
        {
            return Result.Failure<Guid>(CheckpointErrors.NotFound);
        }

        var accessResult = await QuestionAccess.EnsureCanManageCheckpointAsync(
            checkpoint,
            _videoRepository,
            _lessonRepository,
            _courseRepository,
            _userContext,
            cancellationToken);

        if (accessResult.IsFailure)
        {
            return Result.Failure<Guid>(accessResult.Error);
        }

        var checkpointResult = checkpoint.AddQuestion(
            text: command.Text,
            type: command.Type,
            allowRetry: command.AllowRetry,
            timeLimitSeconds: command.TimeLimitSeconds,
            revealCorrectAnswer: command.RevealCorrectAnswer,
            correctTextAnswer: command.CorrectTextAnswer);

        if (checkpointResult.IsFailure)
        {
            return Result.Failure<Guid>(checkpointResult.Error);
        }

        _questionRepository.Add(checkpointResult.Value);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(checkpointResult.Value.Id);
    }
}
