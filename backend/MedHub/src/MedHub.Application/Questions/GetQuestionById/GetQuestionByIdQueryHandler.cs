using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Questions.GetQuestionById;

internal sealed class GetQuestionByIdQueryHandler
    : IQueryHandler<GetQuestionByIdQuery, QuestionResponse>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserContext _userContext;

    public GetQuestionByIdQueryHandler(
        IQuestionRepository questionRepository,
        ICheckpointRepository checkpointRepository,
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IUserContext userContext)
    {
        _questionRepository = questionRepository;
        _checkpointRepository = checkpointRepository;
        _videoRepository = videoRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _userContext = userContext;
    }

    public async Task<Result<QuestionResponse>> Handle(
        GetQuestionByIdQuery query,
        CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(
            query.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure<QuestionResponse>(QuestionErrors.NotFound);
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
            return Result.Failure<QuestionResponse>(accessResult.Error);
        }

        var response = new QuestionResponse(
            question.Id,
            question.CheckpointId,
            question.Text.Value,
            question.Type,
            question.AllowRetry,
            question.TimeLimitSeconds,
            question.RevealCorrectAnswer,
            question.CorrectTextAnswer,
            question.AnswerOptions
                .Select(x => new AnswerOptionResponse(
                    x.Id,
                    x.Text.Value,
                    x.IsCorrect))
                .ToList());

        return Result.Success(response);
    }
}
