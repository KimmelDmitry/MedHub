using MedHub.Domain.Checkpoints;

namespace MedHub.Domain.Checkpoints;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Question>> GetByCheckpointIdAsync(
        Guid checkpointId,
        CancellationToken cancellationToken = default);

    void Remove(Question question);
}