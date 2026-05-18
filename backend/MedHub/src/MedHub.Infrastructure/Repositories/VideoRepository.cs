using MedHub.Domain.Media;
using Microsoft.EntityFrameworkCore;

namespace MedHub.Infrastructure.Repositories;

internal sealed class VideoRepository : Repository<VideoMaterial>, IVideoRepository
{
    
    private readonly ApplicationDbContext _dbContext;

    public VideoRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(VideoMaterial video, CancellationToken ct = default)
    {
        await _dbContext.Set<VideoMaterial>().AddAsync(video, ct);
    }

    public async Task UpdateAsync(VideoMaterial video, CancellationToken ct = default)
    {
        _dbContext.Entry(video).State = EntityState.Modified;
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(VideoMaterial video, CancellationToken ct = default)
    {
        _dbContext.Set<VideoMaterial>().Remove(video);
        await Task.CompletedTask;
    }

    public async Task<VideoMaterial?> GetNextUploadedAsync(CancellationToken ct = default)
    {
        return await _dbContext.Set<VideoMaterial>().Where(v => v.Status == VideoStatus.Uploaded)
            .OrderBy(v => v.CreatedAt).FirstOrDefaultAsync(ct);
    }

    public async Task<bool> TryClaimForProcessingAsync(Guid videoId, CancellationToken ct = default)
    {
        var affectedRows = await _dbContext.Set<VideoMaterial>()
            .Where(v => v.Id == videoId && v.Status == VideoStatus.Uploaded).ExecuteUpdateAsync(
                setters => setters.SetProperty(v => v.Status, VideoStatus.Processing)
                    .SetProperty(v => v.UpdatedAt, DateTime.UtcNow), ct);
        return affectedRows > 0;
    }
}