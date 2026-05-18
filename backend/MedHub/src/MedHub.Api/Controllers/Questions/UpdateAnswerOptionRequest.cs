namespace MedHub.Api.Controllers.Questions;

public sealed record UpdateAnswerOptionRequest(
    string Text,
    bool IsCorrect);