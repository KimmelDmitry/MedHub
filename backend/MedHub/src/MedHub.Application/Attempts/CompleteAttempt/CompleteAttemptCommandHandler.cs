using MedHub.Application.Abstractions.Clock;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Attempts;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Attempts.CompleteAttempt;

internal sealed class CompleteAttemptCommandHandler
    : ICommandHandler<CompleteAttemptCommand, CompleteAttemptResponse>
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CompleteAttemptCommandHandler(
        IAttemptRepository attemptRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _attemptRepository = attemptRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<CompleteAttemptResponse>> Handle(
        CompleteAttemptCommand command,
        CancellationToken cancellationToken)
    {
        Attempt? attempt = await _attemptRepository.GetByIdAsync(
            command.AttemptId,
            cancellationToken);

        if (attempt is null)
            return Result.Failure<CompleteAttemptResponse>(
                AttemptErrors.NotFound);
        
        
        decimal score = CalculateScore(attempt);

        Result result = attempt.Complete(score, _dateTimeProvider.UtcNow);

        if (result.IsFailure)
            return Result.Failure<CompleteAttemptResponse>(
                result.Error);

        return Result.Success(
            new CompleteAttemptResponse(
                attempt.Id,
                attempt.Score,
                attempt.CompletedAt!.Value,
                attempt.Status.ToString()));
    }
    
    private static decimal CalculateScore(Attempt attempt)
    {
        if (!attempt.Answers.Any())
        {
            return 0;
        }

        int correctAnswers = attempt.Answers.Count(x => x.IsCorrect);

        decimal score =
            (decimal)correctAnswers / attempt.Answers.Count * 100;

        return Math.Round(score, 2);
    }
}