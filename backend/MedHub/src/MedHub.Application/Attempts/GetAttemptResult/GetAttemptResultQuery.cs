using MedHub.Application.Abstractions.Messaging;

namespace MedHub.Application.Attempts.GetAttemptResult;

public sealed record GetAttemptResultQuery(
    Guid AttemptId
) : IQuery<AttemptResultResponse>;