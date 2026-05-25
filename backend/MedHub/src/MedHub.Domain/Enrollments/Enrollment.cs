using MedHub.Domain.Abstractions;
using MedHub.Domain.Enrollments.Events;

namespace MedHub.Domain.Enrollments;

public sealed class Enrollment : Entity
{
    private Enrollment()
    {
    }

    private Enrollment(
        Guid id,
        Guid studentId,
        Guid courseId,
        DateTime enrolledAtUtc)
        : base(id)
    {
        StudentId = studentId;
        CourseId = courseId;
        Status = EnrollmentStatus.Active;
        EnrolledAtUtc = enrolledAtUtc;
    }

    public Guid StudentId { get; private set; }
    public Guid CourseId { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public DateTime EnrolledAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public static Result<Enrollment> Create(
        Guid studentId,
        Guid courseId,
        DateTime utcNow)
    {
        if (studentId == Guid.Empty)
        {
            return Result.Failure<Enrollment>(EnrollmentErrors.InvalidStudentId);
        }

        if (courseId == Guid.Empty)
        {
            return Result.Failure<Enrollment>(EnrollmentErrors.InvalidCourseId);
        }

        var enrollment = new Enrollment(
            Guid.NewGuid(),
            studentId,
            courseId,
            utcNow);

        enrollment.RaiseDomainEvent(
            new EnrollmentCreatedEvent(enrollment.Id, studentId, courseId));

        return Result.Success(enrollment);
    }

    public Result Reactivate(DateTime utcNow)
    {
        if (Status == EnrollmentStatus.Active)
        {
            return Result.Success();
        }

        Status = EnrollmentStatus.Active;
        CancelledAtUtc = null;
        CompletedAtUtc = null;
        UpdatedAtUtc = utcNow;

        RaiseDomainEvent(new EnrollmentCreatedEvent(Id, StudentId, CourseId));

        return Result.Success();
    }

    public Result Complete(DateTime utcNow)
    {
        if (Status == EnrollmentStatus.Completed)
        {
            return Result.Failure(EnrollmentErrors.CannotComplete);
        }

        if (Status != EnrollmentStatus.Active)
        {
            return Result.Failure(EnrollmentErrors.NotActive);
        }

        Status = EnrollmentStatus.Completed;
        CompletedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;

        RaiseDomainEvent(new EnrollmentCompletedEvent(Id));

        return Result.Success();
    }

    public Result Cancel(DateTime utcNow)
    {
        if (Status == EnrollmentStatus.Cancelled)
        {
            return Result.Success();
        }

        if (Status != EnrollmentStatus.Active)
        {
            return Result.Failure(EnrollmentErrors.CannotCancel);
        }

        Status = EnrollmentStatus.Cancelled;
        CancelledAtUtc = utcNow;
        UpdatedAtUtc = utcNow;

        RaiseDomainEvent(new EnrollmentCancelledEvent(Id));

        return Result.Success();
    }

    public bool IsActive() => Status == EnrollmentStatus.Active;
}
