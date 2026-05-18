using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;
using MedHub.Domain.Users; // Для IUserContext

namespace MedHub.Application.Courses.CreateCourse;

internal sealed class CreateCourseCommandHandler : ICommandHandler<CreateCourseCommand, Guid>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public CreateCourseCommandHandler(
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<Guid>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        // 1. Получаем ID текущего пользователя (Преподаватель)
        // В реальном приложении здесь также можно проверить роль "Teacher", 
        // но пока полагаемся на то, что эндпоинт защищен правами.
        var creatorId = _userContext.UserId;

        // 2. Создаем сущность через фабрику (Domain Logic)
        var createResult = Course.Create(request.Title, request.Description, creatorId);
        
        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Error);
        }

        var course = createResult.Value;

        // 3. Сохраняем в репозиторий
        _courseRepository.Add(course);

        // 4. Сохраняем изменения в БД (триггерит Outbox для событий)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Возвращаем ID созданного курса
        return course.Id;
    }
}