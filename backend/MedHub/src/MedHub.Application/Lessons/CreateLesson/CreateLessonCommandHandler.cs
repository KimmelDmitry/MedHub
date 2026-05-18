using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons;
using MedHub.Domain.Users;

namespace MedHub.Application.Lessons.CreateLesson;

internal sealed class CreateLessonCommandHandler : ICommandHandler<CreateLessonCommand, Guid>
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public CreateLessonCommandHandler(
        ICourseRepository courseRepository,
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _courseRepository = courseRepository;
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<Guid>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        if (!_userContext.IsInRole("Teacher"))
        {
            return Result.Failure<Guid>(new Error(
                "Lesson.CreatorNotTeacher",
                "Только преподаватели могут создавать уроки"));
        }

        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course is null)
        {
            return Result.Failure<Guid>(CourseErrors.NotFound); 
        }

        if (course.CreatorId != _userContext.UserId)
        {
            return Result.Failure<Guid>(new Error(
                "Lesson.CreatorNotTeacher",
                "только автор курса может добавлять уроки к своему курсу"));
        }

        var createResult = Lesson.Create(
            request.CourseId,
            request.Title,
            request.Order,
            request.ContentType,
            request.ContentUrl);

        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Error);
        }

        var lesson = createResult.Value;

        _lessonRepository.Add(lesson);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lesson.Id;
    }
}