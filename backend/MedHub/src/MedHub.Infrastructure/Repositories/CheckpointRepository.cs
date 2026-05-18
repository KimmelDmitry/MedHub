using MedHub.Domain.Checkpoints;
using MedHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedHub.Infrastructure.Repositories;

internal sealed class CheckpointRepository : Repository<Checkpoint>, ICheckpointRepository
{
    public CheckpointRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Checkpoint?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Checkpoint>()
            .Include(x => x.Questions)
            .ThenInclude(x => x.AnswerOptions)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Checkpoint>> GetByVideoIdAsync(
        Guid videoId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Checkpoint>()
            .Include(x => x.Questions)
            .ThenInclude(x => x.AnswerOptions)
            .Where(x => x.VideoId == videoId)
            .OrderBy(x => x.Timestamp)
            .ToListAsync<Checkpoint>(cancellationToken);
    }

    public void Add(Checkpoint checkpoint)
    {
        DbContext.Set<Checkpoint>().Add(checkpoint);
    }

    public void Remove(Checkpoint checkpoint)
    {
        DbContext.Set<Checkpoint>().Remove(checkpoint);
    }
}