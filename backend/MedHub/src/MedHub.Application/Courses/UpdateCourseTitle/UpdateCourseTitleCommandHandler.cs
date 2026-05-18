using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;

namespace MedHub.Application.Courses.UpdateCourseTitle;

internal sealed class UpdateCourseTitleCommandHandler
    : ICommandHandler<UpdateCourseTitleCommand>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCourseTitleCommandHandler(
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateCourseTitleCommand command,
        CancellationToken cancellationToken)
    {
        Course? course = await _courseRepository.GetByIdAsync(
            command.CourseId,
            cancellationToken);

        if (course is null)
        {
            return Result.Failure(CourseErrors.NotFound);
        }

        Result result = course.UpdateTitle(command.Title);

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}