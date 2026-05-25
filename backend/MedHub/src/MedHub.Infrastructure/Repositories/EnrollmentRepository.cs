using MedHub.Domain.Enrollments;
using Microsoft.EntityFrameworkCore;

namespace MedHub.Infrastructure.Repositories;

internal sealed class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Enrollment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Enrollment>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Enrollment?> GetByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Enrollment>()
            .FirstOrDefaultAsync(
                x => x.StudentId == studentId && x.CourseId == courseId,
                cancellationToken);
    }

    public async Task<bool> IsActiveAsync(
        Guid studentId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Enrollment>()
            .AnyAsync(
                x =>
                    x.StudentId == studentId &&
                    x.CourseId == courseId &&
                    x.Status == EnrollmentStatus.Active,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Enrollment>> ListByStudentAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Enrollment>()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.EnrolledAtUtc)
            .ToListAsync(cancellationToken);
    }

    public void Add(Enrollment enrollment)
    {
        DbContext.Set<Enrollment>().Add(enrollment);
    }
}
