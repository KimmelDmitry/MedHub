namespace MedHub.Domain.Lessons;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Lesson lesson);
    // Специфичный метод, которого нет в базовом классе
    Task<IReadOnlyList<Lesson>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);
}