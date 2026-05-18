using MedHub.Domain.Courses;
using Microsoft.EntityFrameworkCore;

namespace MedHub.Infrastructure.Repositories;

internal sealed class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Course>()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Course?> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Course>()
            .Where(c => c.Id == Id)
            .OrderByDescending(c  => c.CreatedAt)
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
