using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Clock;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Attempts;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Courses;
using MedHub.Domain.Enrollments;
using MedHub.Domain.Lessons;

namespace MedHub.Application.Attempts.CompleteAttempt;

internal sealed class CompleteAttemptCommandHandler
    : ICommandHandler<CompleteAttemptCommand, CompleteAttemptResponse>
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteAttemptCommandHandler(
        IAttemptRepository attemptRepository,
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _attemptRepository = attemptRepository;
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _userContext = userContext;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CompleteAttemptResponse>> Handle(
        CompleteAttemptCommand command,
        CancellationToken cancellationToken)
    {
        Attempt? attempt = await _attemptRepository.GetByIdAsync(
            command.AttemptId,
            cancellationToken);

        if (attempt is null)
        {
            return Result.Failure<CompleteAttemptResponse>(
                AttemptErrors.NotFound);
        }

        if (attempt.StudentId != _userContext.UserId)
        {
            return Result.Failure<CompleteAttemptResponse>(
                new Error(
                    "Attempt.Forbidden",
                "Only the attempt owner can complete the attempt."));
        }

        Lesson? lesson = await _lessonRepository.GetByIdAsync(
            attempt.LessonId,
            cancellationToken);

        if (lesson is null)
        {
            return Result.Failure<CompleteAttemptResponse>(
                LessonErrors.NotFound);
        }

        Course? course = await _courseRepository.GetByIdAsync(
            lesson.CourseId,
            cancellationToken);

        if (course is null)
        {
            return Result.Failure<CompleteAttemptResponse>(
                CourseErrors.NotFound);
        }

        bool hasActiveEnrollment = await _enrollmentRepository.IsActiveAsync(
            attempt.StudentId,
            course.Id,
            cancellationToken);

        if (!hasActiveEnrollment)
        {
            return Result.Failure<CompleteAttemptResponse>(
                EnrollmentErrors.Required);
        }

        decimal score = CalculateScore(attempt);

        Result result = attempt.Complete(score, _dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return Result.Failure<CompleteAttemptResponse>(
                result.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new CompleteAttemptResponse(
                attempt.Id,
                attempt.Score,
                attempt.CompletedAt!.Value,
                attempt.Status.ToString()));
    }

    private static decimal CalculateScore(Attempt attempt)
    {
        if (!attempt.Answers.Any())
        {
            return 0;
        }

        int correctAnswers = attempt.Answers.Count(x => x.IsCorrect);

        decimal score =
            (decimal)correctAnswers / attempt.Answers.Count * 100;

        return Math.Round(score, 2);
    }
}
