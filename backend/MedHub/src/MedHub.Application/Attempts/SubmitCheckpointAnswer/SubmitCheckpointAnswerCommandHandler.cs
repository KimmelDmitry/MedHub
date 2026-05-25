using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Clock;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Attempts;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Enums;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Courses;
using MedHub.Domain.Enrollments;
using MedHub.Domain.Lessons;
using MedHub.Domain.Media;

namespace MedHub.Application.Attempts.SubmitCheckpointAnswer;

internal sealed class SubmitCheckpointAnswerCommandHandler
    : ICommandHandler<SubmitCheckpointAnswerCommand, SubmitCheckpointAnswerResponse>
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitCheckpointAnswerCommandHandler(
        IAttemptRepository attemptRepository,
        IQuestionRepository questionRepository,
        ICheckpointRepository checkpointRepository,
        IVideoRepository videoRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _attemptRepository = attemptRepository;
        _questionRepository = questionRepository;
        _checkpointRepository = checkpointRepository;
        _videoRepository = videoRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _userContext = userContext;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubmitCheckpointAnswerResponse>> Handle(
        SubmitCheckpointAnswerCommand command,
        CancellationToken cancellationToken)
    {
        Attempt? attempt = await _attemptRepository.GetByIdAsync(
            command.AttemptId,
            cancellationToken);

        if (attempt is null)
        {
            return Result.Failure<SubmitCheckpointAnswerResponse>(
                AttemptErrors.NotFound);
        }

        if (attempt.StudentId != _userContext.UserId)
        {
            return Result.Failure<SubmitCheckpointAnswerResponse>(
                new Error(
                    "Attempt.Forbidden",
                    "Only the attempt owner can submit answers."));
        }

        Question? question = await _questionRepository.GetByIdAsync(
            command.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure<SubmitCheckpointAnswerResponse>(
                QuestionErrors.NotFound);
        }

        Result validationResult = await EnsureQuestionBelongsToAttemptLessonAsync(
            attempt,
            question,
            cancellationToken);

        if (validationResult.IsFailure)
        {
            return Result.Failure<SubmitCheckpointAnswerResponse>(
                validationResult.Error);
        }

        DateTime nowUtc = _dateTimeProvider.UtcNow;

        Result<Answer> result = attempt.SubmitAnswer(
            question,
            command.SelectedOptionIds,
            command.TextAnswer,
            nowUtc);

        if (result.IsFailure)
        {
            return Result.Failure<SubmitCheckpointAnswerResponse>(
                result.Error);
        }

        Answer answer = result.Value;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new SubmitCheckpointAnswerResponse(
                answer.Id,
                answer.IsCorrect,
                attempt.Score));
    }

    private async Task<Result> EnsureQuestionBelongsToAttemptLessonAsync(
        Attempt attempt,
        Question question,
        CancellationToken cancellationToken)
    {
        Checkpoint? checkpoint = await _checkpointRepository.GetByIdAsync(
            question.CheckpointId,
            cancellationToken);

        if (checkpoint is null)
        {
            return Result.Failure(CheckpointErrors.NotFound);
        }

        VideoMaterial? video = await _videoRepository.GetByIdAsync(
            checkpoint.VideoId,
            cancellationToken);

        if (video is null)
        {
            return Result.Failure(VideoErrors.NotFound);
        }

        Lesson? lesson = await _lessonRepository.GetByIdAsync(
            video.LessonId,
            cancellationToken);

        if (lesson is null)
        {
            return Result.Failure(LessonErrors.NotFound);
        }

        if (lesson.Id != attempt.LessonId)
        {
            return Result.Failure(AttemptErrors.QuestionMismatch);
        }

        Course? course = await _courseRepository.GetByIdAsync(
            lesson.CourseId,
            cancellationToken);

        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        if (checkpoint.Status != CheckpointStatus.Published ||
            lesson.Status != LessonStatus.Published ||
            course.Status != CourseStatus.Published ||
            lesson.VideoId != video.Id ||
            video.Status != VideoStatus.Ready)
        {
            return Result.Failure(
                new Error(
                    "Attempt.Forbidden",
                    "Answers can be submitted only for published runtime content."));
        }

        bool hasActiveEnrollment = await _enrollmentRepository.IsActiveAsync(
            attempt.StudentId,
            course.Id,
            cancellationToken);

        if (!hasActiveEnrollment)
        {
            return Result.Failure(EnrollmentErrors.Required);
        }

        return Result.Success();
    }
}
