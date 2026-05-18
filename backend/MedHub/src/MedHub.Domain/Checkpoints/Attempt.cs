using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints.Enums;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Checkpoints.Models;

namespace MedHub.Domain.Checkpoints;

public sealed class Attempt : Entity
{
    private Attempt()
    {
    }

    private Attempt(
        Guid id,
        Guid studentId,
        Guid lessonId,
        DateTime startedAt)
        : base(id)
    {
        StudentId = studentId;
        LessonId = lessonId;
        StartedAt = startedAt;
        Status = AttemptStatus.InProgress;
    }

    public Guid StudentId { get; private set; }
    public Guid LessonId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public AttemptStatus Status { get; private set; }
    public decimal Score { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<Answer> _answers = new();
    public IReadOnlyCollection<Answer> Answers => _answers;

    public static Result<Attempt> Create(
        Guid studentId,
        Guid lessonId,
        DateTime utcNow)
    {
        if (studentId == Guid.Empty)
            return Result.Failure<Attempt>(AttemptErrors.InvalidStudentId);
        
        if (lessonId == Guid.Empty)
            return Result.Failure<Attempt>(AttemptErrors.InvalidLessonId);

        var attempt = new Attempt(Guid.NewGuid(), studentId, lessonId, utcNow);
    
        // Create не вызывает RaiseDomainEvent.
        // метод предназначен для чистой инициализации
        // а не для запуска бизнес-сценария.
    
        return Result.Success(attempt);
    }
    
    public static Result<Attempt> Start(
        Guid studentId,
        Guid lessonId,
        DateTime utcNow)
    {
        var attemptResult = Attempt.Create(studentId, lessonId, utcNow);

        if (attemptResult.IsFailure)
        {
            return Result.Failure<Attempt>(attemptResult.Error);
        }
        
        var attempt = attemptResult.Value;
        
        attempt.RaiseDomainEvent(new Events.AttemptStartedEvent(attempt.Id, studentId, lessonId));
        
        return Result.Success(attempt);
    }

    public Result<Answer> SubmitAnswer(
        Question question,
        IReadOnlyCollection<Guid>? selectedOptionIds,
        string? textAnswer,
        DateTime utcNow)
    {
        if (Status != AttemptStatus.InProgress)
        {
            return Result.Failure<Answer>(AttemptErrors.InvalidTransition);
        }

        var existingAnswer = _answers.FirstOrDefault(x => x.QuestionId == question.Id);
        if (existingAnswer is not null && !question.AllowRetry)
        {
            return Result.Failure<Answer>(AttemptErrors.AlreadyAnswered);
        }

        var evaluationResult = question.EvaluateAnswer(selectedOptionIds, textAnswer);
        if (evaluationResult.IsFailure)
        {
            return Result.Failure<Answer>(evaluationResult.Error);
        }

        if (existingAnswer is not null)
        {
            _answers.Remove(existingAnswer);
        }

        var evaluation = evaluationResult.Value;

        var answer = new Answer(
            Guid.NewGuid(),
            Id,
            question.Id,
            selectedOptionIds,
            textAnswer,
            evaluation.IsCorrect,
            evaluation.RequiresManualReview,
            utcNow);

        _answers.Add(answer);
        UpdatedAt = utcNow;

        RaiseDomainEvent(new Events.AttemptAnswerSubmittedEvent(Id, question.Id));

        return Result.Success(answer);
    }

    public Result Complete(decimal score, DateTime utcNow)
    {
        if (Status != AttemptStatus.InProgress)
        {
            return Result.Failure(AttemptErrors.AlreadyCompleted);
        }

        if (score < 0)
        {
            return Result.Failure(AttemptErrors.InvalidScore);
        }

        Score = score;
        CompletedAt = utcNow;
        Status = AttemptStatus.Completed;
        UpdatedAt = utcNow;

        RaiseDomainEvent(new Events.AttemptCompletedEvent(Id, score));

        return Result.Success();
    }

    public Result Fail(string reason, DateTime utcNow)
    {
        if (Status == AttemptStatus.Completed)
        {
            return Result.Failure(AttemptErrors.AlreadyCompleted);
        }

        Status = AttemptStatus.Failed;
        UpdatedAt = utcNow;

        RaiseDomainEvent(new Events.AttemptFailedEvent(Id, reason));

        return Result.Success();
    }

    public Result Cancel(DateTime utcNow)
    {
        if (Status != AttemptStatus.InProgress)
        {
            return Result.Failure(AttemptErrors.InvalidTransition);
        }

        Status = AttemptStatus.Cancelled;
        UpdatedAt = utcNow;

        return Result.Success();
    }

    public bool HasAnswerFor(Guid questionId)
    {
        return _answers.Any(x => x.QuestionId == questionId);
    }
}