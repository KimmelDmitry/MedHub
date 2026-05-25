using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Clock;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Attempts;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Courses;
using MedHub.Domain.Enrollments;
using MedHub.Domain.Lessons;

namespace MedHub.Application.Attempts.StartAttempt;

internal sealed class StartAttemptCommandHandler
    : ICommandHandler<StartAttemptCommand, StartAttemptResponse>
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider  _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public StartAttemptCommandHandler(
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

    public async Task<Result<StartAttemptResponse>> Handle(
        StartAttemptCommand command,
        CancellationToken cancellationToken)
    {
        Lesson? lesson = await _lessonRepository.GetByIdAsync(
            command.LessonId,
            cancellationToken);

        if (lesson is null)
            return Result.Failure<StartAttemptResponse>(LessonErrors.NotFound);

        Course? course = await _courseRepository.GetByIdAsync(
            lesson.CourseId,
            cancellationToken);

        if (course is null)
            return Result.Failure<StartAttemptResponse>(CourseErrors.NotFound);

        if (lesson.Status != LessonStatus.Published ||
            course.Status != CourseStatus.Published)
        {
            return Result.Failure<StartAttemptResponse>(
                new Error(
                    "Attempt.Forbidden",
                    "Only published lessons from published courses can be started by students."));
        }

        Guid studentId = _userContext.UserId;

        bool hasActiveEnrollment = await _enrollmentRepository.IsActiveAsync(
            studentId,
            course.Id,
            cancellationToken);

        if (!hasActiveEnrollment)
        {
            return Result.Failure<StartAttemptResponse>(
                EnrollmentErrors.Required);
        }

        Attempt? existingAttempt =
            await _attemptRepository.GetActiveAttemptAsync(
                studentId,
                command.LessonId,
                cancellationToken);

        if (existingAttempt is not null)
        {
            return Result.Success(
                new StartAttemptResponse(
                    existingAttempt.Id,
                    existingAttempt.LessonId,
                    existingAttempt.StartedAt,
                    existingAttempt.Status.ToString()));
        }

        DateTime utcNow = _dateTimeProvider.UtcNow;

        Result<Attempt> attemptResult = Attempt.Start(studentId, command.LessonId, utcNow);

        if (attemptResult.IsFailure)
        {
            return  Result.Failure<StartAttemptResponse>(attemptResult.Error);
        }

        var attempt = attemptResult.Value;
        
        _attemptRepository.Add(attempt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new StartAttemptResponse(
                attempt.Id,
                attempt.LessonId,
                attempt.StartedAt,
                attempt.Status.ToString()));
    }
}
