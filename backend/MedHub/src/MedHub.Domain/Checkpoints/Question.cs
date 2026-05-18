using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints.Enums;
using MedHub.Domain.Checkpoints.Errors;
using MedHub.Domain.Checkpoints.Models;
using MedHub.Domain.Checkpoints.ValueObjects;

namespace MedHub.Domain.Checkpoints;

public sealed class Question : Entity
{
    private Question()
    {
    }

    internal Question(
        Guid id,
        Guid checkpointId,
        QuestionText text,
        QuestionType type,
        bool allowRetry,
        int? timeLimitSeconds,
        bool revealCorrectAnswer,
        string? correctTextAnswer)
        : base(id)
    {
        CheckpointId = checkpointId;
        Text = text;
        Type = type;
        AllowRetry = allowRetry;
        TimeLimitSeconds = timeLimitSeconds;
        RevealCorrectAnswer = revealCorrectAnswer;
        CorrectTextAnswer = string.IsNullOrWhiteSpace(correctTextAnswer)
            ? null
            : correctTextAnswer.Trim();
    }

    public Guid CheckpointId { get; private set; }
    public QuestionText Text { get; private set; } = null!;
    public QuestionType Type { get; private set; }
    public bool AllowRetry { get; private set; }
    public int? TimeLimitSeconds { get; private set; }
    public bool RevealCorrectAnswer { get; private set; }
    public string? CorrectTextAnswer { get; private set; }

    private readonly List<AnswerOption> _answerOptions = new();
    public IReadOnlyCollection<AnswerOption> AnswerOptions => _answerOptions;

    public static Result<Question> Create(
        Guid checkpointId,
        string text,
        QuestionType type,
        bool allowRetry,
        int? timeLimitSeconds = null,
        bool revealCorrectAnswer = false,
        string? correctTextAnswer = null)
    {
        var textResult = QuestionText.Create(text);
        if (textResult.IsFailure)
        {
            return Result.Failure<Question>(textResult.Error);
        }

        if (timeLimitSeconds is <= 0)
        {
            return Result.Failure<Question>(QuestionErrors.InvalidTimeLimit);
        }

        var question = new Question(
            Guid.NewGuid(),
            checkpointId,
            textResult.Value,
            type,
            allowRetry,
            timeLimitSeconds,
            revealCorrectAnswer,
            correctTextAnswer);

        return Result.Success(question);
    }
    
    public Result UpdateAnswerOption(Guid answerOptionId, string text, bool isCorrect)
    {
        if (Type == QuestionType.Text)
            return Result.Failure(QuestionErrors.TextQuestionCannotHaveOptions);

        AnswerOption? option = _answerOptions.FirstOrDefault(x => x.Id == answerOptionId);
        if (option is null)
            return Result.Failure(QuestionErrors.AnswerOptionNotFound); 

        var textResult = AnswerOptionText.Create(text);
        if (textResult.IsFailure)
            return Result.Failure(textResult.Error);

        option.Update(textResult.Value, isCorrect);

        RaiseDomainEvent(new Events.AnswerOptionUpdatedEvent(Id, answerOptionId));
        return Result.Success();
    }

    public Result UpdateText(string newText)
    {
        var textResult = QuestionText.Create(newText);
        if (textResult.IsFailure)
        {
            return Result.Failure(textResult.Error);
        }

        Text = textResult.Value;
        return Result.Success();
    }

    public Result UpdateSettings(
        bool allowRetry,
        int? timeLimitSeconds,
        bool revealCorrectAnswer,
        string? correctTextAnswer = null)
    {
        if (timeLimitSeconds is <= 0)
        {
            return Result.Failure(QuestionErrors.InvalidTimeLimit);
        }

        AllowRetry = allowRetry;
        TimeLimitSeconds = timeLimitSeconds;
        RevealCorrectAnswer = revealCorrectAnswer;
        CorrectTextAnswer = string.IsNullOrWhiteSpace(correctTextAnswer)
            ? CorrectTextAnswer
            : correctTextAnswer.Trim();

        return Result.Success();
    }

    public Result<AnswerOption> AddOption(string text, bool isCorrect)
    {
        if (Type == QuestionType.Text)
        {
            return Result.Failure<AnswerOption>(QuestionErrors.TextQuestionCannotHaveOptions);
        }

        var textResult = AnswerOptionText.Create(text);
        if (textResult.IsFailure)
        {
            return Result.Failure<AnswerOption>(textResult.Error);
        }

        var option = new AnswerOption(Guid.NewGuid(), Id, textResult.Value, isCorrect);
        _answerOptions.Add(option);

        RaiseDomainEvent(new Events.AnswerOptionAddedEvent(Id, option.Id));

        return Result.Success(option);
    }

    public Result RemoveOption(Guid answerOptionId)
    {
        var option = _answerOptions.FirstOrDefault(x => x.Id == answerOptionId);
        if (option is null)
        {
            return Result.Failure(QuestionErrors.AnswerOptionNotFound);
        }

        _answerOptions.Remove(option);
        RaiseDomainEvent(new Events.AnswerOptionRemovedEvent(Id, answerOptionId));
        return Result.Success();
    }

    public Result ValidateForPublish()
    {
        if (Type == QuestionType.Text)
        {
            return Result.Success();
        }

        if (_answerOptions.Count < 2)
        {
            return Result.Failure(QuestionErrors.NotEnoughOptions);
        }

        var correctCount = _answerOptions.Count(x => x.IsCorrect);

        return Type switch
        {
            QuestionType.SingleChoice when correctCount == 1 => Result.Success(),
            QuestionType.TrueFalse when _answerOptions.Count == 2 && correctCount == 1 => Result.Success(),
            QuestionType.MultipleChoice when correctCount >= 1 => Result.Success(),
            _ => Result.Failure(QuestionErrors.InvalidCorrectCount)
        };
    }

    public Result<QuestionEvaluationResult> EvaluateAnswer(
        IReadOnlyCollection<Guid>? selectedOptionIds,
        string? textAnswer)
    {
        if (Type == QuestionType.Text)
        {
            if (string.IsNullOrWhiteSpace(textAnswer))
            {
                return Result.Failure<QuestionEvaluationResult>(QuestionErrors.InvalidAnswerShape);
            }

            if (string.IsNullOrWhiteSpace(CorrectTextAnswer))
            {
                return Result.Success(new QuestionEvaluationResult(
                    IsCorrect: false,
                    RequiresManualReview: true));
            }

            var actual = textAnswer.Trim();
            var expected = CorrectTextAnswer.Trim();

            var isCorrect = string.Equals(
                actual,
                expected,
                StringComparison.OrdinalIgnoreCase);

            return Result.Success(new QuestionEvaluationResult(
                IsCorrect: isCorrect,
                RequiresManualReview: false));
        }

        if (selectedOptionIds is null || selectedOptionIds.Count == 0)
        {
            return Result.Failure<QuestionEvaluationResult>(QuestionErrors.InvalidAnswerShape);
        }

        var selected = selectedOptionIds.Distinct().ToArray();
        var correctIds = _answerOptions
            .Where(x => x.IsCorrect)
            .Select(x => x.Id)
            .ToArray();

        if (Type is QuestionType.SingleChoice or QuestionType.TrueFalse)
        {
            if (selected.Length != 1)
            {
                return Result.Failure<QuestionEvaluationResult>(QuestionErrors.InvalidAnswerShape);
            }

            var isCorrect = correctIds.Length == 1 && selected[0] == correctIds[0];

            return Result.Success(new QuestionEvaluationResult(
                IsCorrect: isCorrect,
                RequiresManualReview: false));
        }

        if (Type == QuestionType.MultipleChoice)
        {
            var isCorrect = selected.Length == correctIds.Length &&
                            selected.All(id => correctIds.Contains(id)) &&
                            correctIds.All(id => selected.Contains(id));

            return Result.Success(new QuestionEvaluationResult(
                IsCorrect: isCorrect,
                RequiresManualReview: false));
        }

        return Result.Failure<QuestionEvaluationResult>(QuestionErrors.InvalidAnswerShape);
    }

    internal void BindToCheckpoint(Guid checkpointId)
    {
        CheckpointId = checkpointId;
    }
}