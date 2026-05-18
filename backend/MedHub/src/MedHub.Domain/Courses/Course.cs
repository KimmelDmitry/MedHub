using MedHub.Domain.Abstractions;
using MedHub.Domain.Courses.Events;
using MedHub.Domain.Courses.ValueObjects;
using MedHub.Domain.Lessons;
using MedHub.Domain.Users;

namespace MedHub.Domain.Courses;

public sealed class Course : Entity
{
    private Course() { }

    private Course(
        Guid id,
        CourseTitle title,
        CourseDescription description,
        Guid creatorId)
        : base(id)
    {
        Title = title;
        Description = description;
        CreatorId = creatorId;
        Status = CourseStatus.Draft;
        CreatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CourseCreatedEvent(Id, CreatorId));
    }

    public CourseTitle Title { get; private set; } = null!;
    public CourseDescription Description { get; private set; } = null!;
    public Guid CreatorId { get; private set; }
    public CourseStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public ICollection<Lesson> Lessons { get; private set; } = new List<Lesson>();

    public static Result<Course> Create(string title, string? description, Guid creatorId)
    {
        var titleResult = CourseTitle.Create(title);
        if (titleResult.IsFailure)
            return Result.Failure<Course>(titleResult.Error);

        var descriptionResult = CourseDescription.Create(description);
        
        if (descriptionResult.IsFailure)
            return Result.Failure<Course>(descriptionResult.Error);

        var course = new Course(Guid.NewGuid(), titleResult.Value, descriptionResult.Value, creatorId);
        
        return Result.Success(course);
    }

    
    public Result Publish()
    {
        if (Status == CourseStatus.Published)
            return Result.Failure(CourseErrors.AlreadyPublished);

        if (Status == CourseStatus.Archived)
            return Result.Failure(CourseErrors.CannotPublishArchived);
        
        if (!Lessons.Any()) return Result.Failure(CourseErrors.NoLessons);

        Status = CourseStatus.Published;
        
        
        RaiseDomainEvent(new CoursePublishedEvent(Id));

        return Result.Success();
    }

  
    public Result Archive()
    {
        if (Status == CourseStatus.Archived)
            return Result.Success(); // Уже архивирован

        if (Status == CourseStatus.Draft)
            return Result.Failure(CourseErrors.CannotArchiveDraft);

        Status = CourseStatus.Archived;
        RaiseDomainEvent(new CourseArchivedEvent(Id));

        return Result.Success();
    }
    
    public Result UpdateTitle(string newTitle)
    {
        var titleResult = CourseTitle.Create(newTitle);
        if (titleResult.IsFailure)
            return Result.Failure(titleResult.Error);

        Title = titleResult.Value;
        return Result.Success();
    }
    
    public Result UpdateDescription(string? description)
    {
        var result = CourseDescription.Create(description);

        if (result.IsFailure)
            return Result.Failure(result.Error);

        Description = result.Value;

        return Result.Success();
    }
}


