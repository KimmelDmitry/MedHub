using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Questions.GetCheckpointQuestions;

internal sealed class GetCheckpointQuestionsQueryHandler
    : IQueryHandler<GetCheckpointQuestionsQuery, IReadOnlyList<CheckpointQuestionResponse>>
{
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserContext _userContext;

    public GetCheckpointQuestionsQueryHandler(
        ICheckpointRepository checkpointRepository,
        IQuestionRepository questionRepository,
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IUserContext userContext)
    {
        _checkpointRepository = checkpointRepository;
        _questionRepository = questionRepository;
        _videoRepository = videoRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _userContext = userContext;
    }

    public async Task<Result<IReadOnlyList<CheckpointQuestionResponse>>> Handle(
        GetCheckpointQuestionsQuery query,
        CancellationToken cancellationToken)
    {
        var checkpoint = await _checkpointRepository.GetByIdAsync(
            query.CheckpointId,
            cancellationToken);

        if (checkpoint is null)
        {
            return Result.Failure<IReadOnlyList<CheckpointQuestionResponse>>(CheckpointErrors.NotFound);
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
            return Result.Failure<IReadOnlyList<CheckpointQuestionResponse>>(accessResult.Error);
        }

        IReadOnlyList<Question> questions = await _questionRepository.GetByCheckpointIdAsync(
            query.CheckpointId,
            cancellationToken);

        var response = questions
            .OrderBy(x => x.Text.Value)
            .Select(x => new CheckpointQuestionResponse(
                x.Id,
                x.Text.Value,
                x.Type,
                x.AnswerOptions.Count))
            .ToList();

        return Result.Success<IReadOnlyList<CheckpointQuestionResponse>>(response);
    }
}
