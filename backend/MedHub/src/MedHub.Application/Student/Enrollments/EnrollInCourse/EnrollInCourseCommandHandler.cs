using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Clock;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;
using MedHub.Domain.Enrollments;

namespace MedHub.Application.Student.Enrollments.EnrollInCourse;

internal sealed class EnrollInCourseCommandHandler
    : ICommandHandler<EnrollInCourseCommand, EnrollmentResponse>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public EnrollInCourseCommandHandler(
        ICourseRepository courseRepository,
        IEnrollmentRepository enrollmentRepository,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _courseRepository = courseRepository;
        _enrollmentRepository = enrollmentRepository;
        _userContext = userContext;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EnrollmentResponse>> Handle(
        EnrollInCourseCommand command,
        CancellationToken cancellationToken)
    {
        if (!_userContext.IsInRole("Student"))
        {
            return Result.Failure<EnrollmentResponse>(EnrollmentErrors.Forbidden);
        }

        Course? course = await _courseRepository.GetByIdAsync(
            command.CourseId,
            cancellationToken);

        if (course is null)
        {
            return Result.Failure<EnrollmentResponse>(CourseErrors.NotFound);
        }

        if (course.Status != CourseStatus.Published)
        {
            return Result.Failure<EnrollmentResponse>(
                EnrollmentErrors.CourseNotPublished);
        }

        Guid studentId = _userContext.UserId;
        DateTime utcNow = _dateTimeProvider.UtcNow;

        Enrollment? enrollment = await _enrollmentRepository.GetByStudentAndCourseAsync(
            studentId,
            command.CourseId,
            cancellationToken);

        if (enrollment is not null)
        {
            Result reactivateResult = enrollment.Reactivate(utcNow);

            if (reactivateResult.IsFailure)
            {
                return Result.Failure<EnrollmentResponse>(reactivateResult.Error);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(ToResponse(enrollment));
        }

        Result<Enrollment> createResult = Enrollment.Create(
            studentId,
            command.CourseId,
            utcNow);

        if (createResult.IsFailure)
        {
            return Result.Failure<EnrollmentResponse>(createResult.Error);
        }

        enrollment = createResult.Value;

        _enrollmentRepository.Add(enrollment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(enrollment));
    }

    private static EnrollmentResponse ToResponse(Enrollment enrollment)
    {
        return new EnrollmentResponse(
            enrollment.Id,
            enrollment.CourseId,
            enrollment.Status.ToString(),
            enrollment.EnrolledAtUtc);
    }
}
