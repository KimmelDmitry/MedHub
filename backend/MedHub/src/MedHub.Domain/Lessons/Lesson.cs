using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses;
using MedHub.Domain.Lessons.DomainEvents;
using MedHub.Domain.Lessons.ValueObjects;

namespace MedHub.Domain.Lessons;

public sealed class Lesson : Entity
{
    private Lesson() { }

    private Lesson(
        Guid id,
        Guid courseId,
        LessonTitle title,
        LessonOrder order,
        LessonContentType contentType,
        string contentUrl)
        : base(id)
    {
        CourseId = courseId;
        Title = title;
        OrderNumber = order;
        ContentType = contentType;
        ContentUrl = contentUrl;
        Status = LessonStatus.Draft;
        
        RaiseDomainEvent(new LessonCreatedEvent(Id, CourseId));
    }

    public Guid CourseId { get; private set; }
    public LessonTitle Title { get; private set; } = null!;
    public LessonOrder OrderNumber { get; private set; } = null!;
    public LessonContentType ContentType { get; private set; }
    public string ContentUrl { get; private set; } = string.Empty;
    public LessonStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Навигационные свойства 
    // public ICollection<VideoMaterial> VideoMaterials { get; private set; } = new List<VideoMaterial>();
    //public ICollection<Question> Questions { get; private set; } = new List<Question>();

    
    public static Result<Lesson> Create(
        Guid courseId, 
        string title, 
        int order, 
        LessonContentType contentType, 
        string? contentUrl = null)
    {
        var titleResult = LessonTitle.Create(title);
        if (titleResult.IsFailure)
            return Result.Failure<Lesson>(titleResult.Error);

        var orderResult = LessonOrder.Create(order);
        if (orderResult.IsFailure)
            return Result.Failure<Lesson>(orderResult.Error);

        // Если контент URL не передан ставим пустую строку (валидно для черновика)
        var url = contentUrl ?? string.Empty;

        var lesson = new Lesson(
            Guid.NewGuid(),
            courseId,
            titleResult.Value,
            orderResult.Value,
            contentType,
            url);

        return Result.Success(lesson);
    }

   
    public Result Publish()
    {
        if (Status == LessonStatus.Published)
            return Result.Success(); 

        if (Status == LessonStatus.Archived)
            return Result.Failure(LessonErrors.InvalidStatusTransition);

        // нельзя публиковать урок без контента
        if (string.IsNullOrEmpty(ContentUrl)) 
             return Result.Failure(LessonErrors.NoContent);

        Status = LessonStatus.Published;
        RaiseDomainEvent(new LessonPublishedEvent(Id, CourseId));

        return Result.Success();
    }

    
    public Result Archive()
    {
        if (Status == LessonStatus.Archived)
            return Result.Success();

        Status = LessonStatus.Archived;
        RaiseDomainEvent(new LessonArchivedEvent(Id, CourseId));

        return Result.Success();
    }

  
    // Обновление порядка урока
    public Result UpdateOrder(int newOrder)
    {
        var orderResult = LessonOrder.Create(newOrder);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Error);

        OrderNumber = orderResult.Value;
        return Result.Success();
    }

    
    public Result UpdateContent(string newUrl, LessonContentType newType)
    {
        if (string.IsNullOrWhiteSpace(newUrl))
            return Result.Failure(LessonErrors.EmptyUrl); // Или специальная ошибка

        ContentUrl = newUrl;
        ContentType = newType;
        
        // поднять событие LessonContentUpdated если нужно кэширование
        return Result.Success();
    }
}

