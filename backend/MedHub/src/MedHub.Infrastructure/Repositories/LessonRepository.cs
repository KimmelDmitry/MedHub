using MedHub.Domain.Lessons;
using Microsoft.EntityFrameworkCore;

namespace MedHub.Infrastructure.Repositories;

internal sealed class LessonRepository : Repository<Lesson>, ILessonRepository
{
    public LessonRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Lesson>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Lesson>()
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderNumber)
            .ToListAsync(cancellationToken);
    }
}