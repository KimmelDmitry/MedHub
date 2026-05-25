using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Questions.AddAnswerOption;

internal sealed class AddAnswerOptionCommandHandler
    : ICommandHandler<AddAnswerOptionCommand, Guid>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public AddAnswerOptionCommandHandler(
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

    public async Task<Result<Guid>> Handle(
        AddAnswerOptionCommand command,
        CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(
            command.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure<Guid>(QuestionErrors.NotFound);
        }

        var accessResult = await QuestionAccess.EnsureCanManageQuestionAsync(
            question,
            _checkpointRepository,
            _videoRepository,
            _lessonRepository,
            _courseRepository,
            _userContext,
            cancellationToken);

        if (accessResult.IsFailure)
        {
            return Result.Failure<Guid>(accessResult.Error);
        }

        var result = question.AddOption(command.Text, command.IsCorrect);
        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        _questionRepository.AddAnswerOption(result.Value);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}
