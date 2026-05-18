using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints.Enums;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Checkpoints.ValueObjects;

namespace MedHub.Domain.Checkpoints;

public sealed class Checkpoint : Entity
{
    private Checkpoint()
    {
    }

    private Checkpoint(
        Guid id,
        Guid videoId,
        CheckpointTimestamp timestamp,
        int orderNumber,
        string? title,
        bool isRequired,
        bool isGraded)
        : base(id)
    {
        VideoId = videoId;
        Timestamp = timestamp;
        OrderNumber = orderNumber;
        Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        IsRequired = isRequired;
        IsGraded = isGraded;
        Status = CheckpointStatus.Draft;
        CreatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.CheckpointCreatedEvent(Id, VideoId));
    }

    public Guid VideoId { get; private set; }
    public CheckpointTimestamp Timestamp { get; private set; } = null!;
    public int OrderNumber { get; private set; }
    public string? Title { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsGraded { get; private set; }
    public CheckpointStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<Question> _questions = new();
    public IReadOnlyCollection<Question> Questions => _questions;

    public static Result<Checkpoint> Create(
        Guid videoId,
        int timestampSeconds,
        int orderNumber,
        int videoDurationSeconds,
        string? title = null,
        bool isRequired = true,
        bool isGraded = true)
    {
        var timestampResult = CheckpointTimestamp.Create(timestampSeconds, videoDurationSeconds);
        if (timestampResult.IsFailure)
        {
            return Result.Failure<Checkpoint>(timestampResult.Error);
        }

        var checkpoint = new Checkpoint(
            Guid.NewGuid(),
            videoId,
            timestampResult.Value,
            orderNumber,
            title,
            isRequired,
            isGraded);

        return Result.Success(checkpoint);
    }

    public Result UpdateTitle(string? title)
    {
        Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.CheckpointUpdatedEvent(Id));
        return Result.Success();
    }

    public Result UpdateFlags(bool isRequired, bool isGraded)
    {
        IsRequired = isRequired;
        IsGraded = isGraded;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.CheckpointUpdatedEvent(Id));
        return Result.Success();
    }

    public Result UpdateTimestamp(int timestampSeconds, int videoDurationSeconds)
    {
        var timestampResult = CheckpointTimestamp.Create(timestampSeconds, videoDurationSeconds);
        if (timestampResult.IsFailure)
        {
            return Result.Failure(timestampResult.Error);
        }

        Timestamp = timestampResult.Value;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.CheckpointUpdatedEvent(Id));
        return Result.Success();
    }

    public Result UpdateOrder(int orderNumber)
    {
        if (orderNumber <= 0)
        {
            return Result.Failure(CheckpointErrors.InvalidOrderNumber);
        }

        OrderNumber = orderNumber;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.CheckpointUpdatedEvent(Id));
        return Result.Success();
    }

    public Result<Question> AddQuestion(
        string text,
        QuestionType type,
        bool allowRetry,
        int? timeLimitSeconds = null,
        bool revealCorrectAnswer = false,
        string? correctTextAnswer = null)
    {
        var questionResult = Question.Create(
            Id,
            text,
            type,
            allowRetry,
            timeLimitSeconds,
            revealCorrectAnswer,
            correctTextAnswer);

        if (questionResult.IsFailure)
        {
            return Result.Failure<Question>(questionResult.Error);
        }

        var question = questionResult.Value;
        question.BindToCheckpoint(Id);
        _questions.Add(question);

        RaiseDomainEvent(new Events.QuestionAddedEvent(Id, question.Id));

        return Result.Success(question);
    }

   

    public Result RemoveQuestion(Guid questionId)
    {
        var question = _questions.FirstOrDefault(x => x.Id == questionId);
        if (question is null)
        {
            return Result.Failure(CheckpointErrors.QuestionNotFound);
        }

        _questions.Remove(question);
        RaiseDomainEvent(new Events.QuestionRemovedEvent(Id, questionId));

        return Result.Success();
    }

    public Result Publish()
    {
        if (Status == CheckpointStatus.Published)
        {
            return Result.Success();
        }

        if (Status == CheckpointStatus.Archived)
        {
            return Result.Failure(CheckpointErrors.InvalidTransition);
        }

        if (IsGraded && _questions.Count == 0)
        {
            return Result.Failure(CheckpointErrors.GradedCheckpointRequiresQuestions);
        }

        foreach (var question in _questions)
        {
            var validation = question.ValidateForPublish();
            if (validation.IsFailure)
            {
                return Result.Failure(validation.Error);
            }
        }

        Status = CheckpointStatus.Published;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.CheckpointPublishedEvent(Id));
        return Result.Success();
    }

    public Result Archive()
    {
        if (Status == CheckpointStatus.Archived)
        {
            return Result.Success();
        }

        Status = CheckpointStatus.Archived;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new Events.CheckpointArchivedEvent(Id));
        return Result.Success();
    }
}