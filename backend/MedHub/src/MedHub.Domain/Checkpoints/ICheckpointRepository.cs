namespace MedHub.Domain.Checkpoints;

public interface ICheckpointRepository
{
    Task<Checkpoint?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Checkpoint>> GetByVideoIdAsync(
        Guid videoId,
        CancellationToken cancellationToken = default);

    void Add(Checkpoint checkpoint);

    void Remove(Checkpoint checkpoint);
}