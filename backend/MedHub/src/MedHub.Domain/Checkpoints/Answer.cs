using MedHub.Domain.Abstractions;

public sealed class Answer : Entity
{
    private readonly List<Guid> _selectedOptionIds = new();

    private Answer()
    {
    }

    internal Answer(
        Guid id,
        Guid attemptId,
        Guid questionId,
        IReadOnlyCollection<Guid>? selectedOptionIds,
        string? textAnswer,
        bool isCorrect,
        bool requiresManualReview,
        DateTime answeredAt)
        : base(id)
    {
        AttemptId = attemptId;
        QuestionId = questionId;
        TextAnswer = string.IsNullOrWhiteSpace(textAnswer) ? null : textAnswer.Trim();
        IsCorrect = isCorrect;
        RequiresManualReview = requiresManualReview;
        AnsweredAt = answeredAt;

        if (selectedOptionIds is not null)
        {
            _selectedOptionIds.AddRange(selectedOptionIds.Distinct());
        }
    }

    public Guid AttemptId { get; private set; }
    public Guid QuestionId { get; private set; }

    public IReadOnlyList<Guid> SelectedOptionIds => _selectedOptionIds;

    public string? TextAnswer { get; private set; }
    public bool IsCorrect { get; private set; }
    public bool RequiresManualReview { get; private set; }
    public DateTime AnsweredAt { get; private set; }
}