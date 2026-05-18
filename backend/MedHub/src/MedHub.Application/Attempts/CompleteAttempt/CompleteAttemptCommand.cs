using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Attempts.CompleteAttempt;

public sealed record CompleteAttemptCommand(
    Guid AttemptId
) : ICommand<CompleteAttemptResponse>;