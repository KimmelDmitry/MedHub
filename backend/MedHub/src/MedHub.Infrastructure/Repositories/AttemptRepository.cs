using MedHub.Domain.Attempts;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Enums;
using MedHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedHub.Infrastructure.Repositories;

internal sealed class AttemptRepository : Repository<Attempt>, IAttemptRepository
{
    public AttemptRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Attempt?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Attempt>()
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Attempt>> GetByStudentIdAsync(
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Attempt>()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Attempt>> GetByLessonIdAsync(
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Attempt>()
            .Where(x => x.LessonId == lessonId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Attempt?> GetByIdWithAnswersAsync(Guid id, CancellationToken ct = default)
    {
        return await DbContext.Set<Attempt>()
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<Attempt?> GetActiveAttemptAsync(
        Guid studentId,
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Attempt>()
            .FirstOrDefaultAsync(
                x =>
                    x.StudentId == studentId &&
                    x.LessonId == lessonId &&
                    x.Status == AttemptStatus.InProgress,
                cancellationToken);
    }

    public void Add(Attempt attempt)
    {
        DbContext.Set<Attempt>().Add(attempt);
    }
}