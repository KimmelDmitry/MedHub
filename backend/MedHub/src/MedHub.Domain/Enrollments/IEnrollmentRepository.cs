namespace MedHub.Domain.Enrollments;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Enrollment?> GetByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<bool> IsActiveAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Enrollment>> ListByStudentAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);

    void Add(Enrollment enrollment);
}
