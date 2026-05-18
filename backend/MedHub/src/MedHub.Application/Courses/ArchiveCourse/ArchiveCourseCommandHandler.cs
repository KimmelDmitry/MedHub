using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;

namespace MedHub.Application.Courses.ArchiveCourse;

internal sealed class ArchiveCourseCommandHandler
    : ICommandHandler<ArchiveCourseCommand>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveCourseCommandHandler(
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ArchiveCourseCommand command,
        CancellationToken cancellationToken)
    {
        Course? course = await _courseRepository.GetByIdAsync(
            command.CourseId,
            cancellationToken);

        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        Result result = course.Archive();

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}