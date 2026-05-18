namespace MedHub.Domain.Courses;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Course course);
    Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken = default);
}