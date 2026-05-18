using MedHub.Domain.Checkpoints;

namespace MedHub.Domain.Attempts;

public interface IAttemptRepository
{
    Task<Attempt?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Attempt>> GetByStudentIdAsync(
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Attempt>> GetByLessonIdAsync(
        Guid lessonId,
        CancellationToken cancellationToken = default);
    
    Task<Attempt?> GetByIdWithAnswersAsync(
        Guid id, 
        CancellationToken ct = default);

    Task<Attempt?> GetActiveAttemptAsync(
        Guid studentId,
        Guid lessonId,
        CancellationToken cancellationToken = default);

    void Add(Attempt attempt);
}