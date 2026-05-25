using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Questions.DeleteQuestion;

internal sealed class DeleteQuestionCommandHandler
    : ICommandHandler<DeleteQuestionCommand>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteQuestionCommandHandler(
        IQuestionRepository questionRepository,
        ICheckpointRepository checkpointRepository,
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _checkpointRepository = checkpointRepository;
        _videoRepository = videoRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(
            command.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure(QuestionErrors.NotFound);
        }

        var checkpoint = await _checkpointRepository.GetByIdAsync(
            question.CheckpointId,
            cancellationToken);

        if (checkpoint is null)
        {
            return Result.Failure(CheckpointErrors.NotFound);
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
            return accessResult;
        }

        var result = checkpoint.RemoveQuestion(question.Id);
        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
