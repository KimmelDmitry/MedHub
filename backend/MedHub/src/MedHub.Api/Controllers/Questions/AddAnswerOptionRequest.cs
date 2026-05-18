namespace MedHub.Api.Controllers.Questions;

public sealed record AddAnswerOptionRequest(
    string Text,
    bool IsCorrect);