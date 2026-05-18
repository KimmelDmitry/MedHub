namespace MedHub.Domain.Media;

public interface IVideoRepository
{
    Task<VideoMaterial?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(VideoMaterial video, CancellationToken ct = default);

    Task UpdateAsync(VideoMaterial video, CancellationToken ct = default);

    Task DeleteAsync(VideoMaterial video, CancellationToken ct = default);

    Task<VideoMaterial?> GetNextUploadedAsync(CancellationToken ct = default);

    Task<bool> TryClaimForProcessingAsync(Guid videoId, CancellationToken ct = default);
}
