namespace MedHub.Api.Controllers.Attempts;

public sealed record SubmitCheckpointAnswerRequest(
    Guid QuestionId,
    IReadOnlyList<Guid> SelectedOptionIds,
    string? TextAnswer);