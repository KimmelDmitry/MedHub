using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Attempts;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Attempts.GetAttemptResult;

internal sealed class GetAttemptResultQueryHandler
    : IQueryHandler<GetAttemptResultQuery, AttemptResultResponse>
{
    private readonly IAttemptRepository _attemptRepository;

    public GetAttemptResultQueryHandler(
        IAttemptRepository attemptRepository)
    {
        _attemptRepository = attemptRepository;
    }

    public async Task<Result<AttemptResultResponse>> Handle(
        GetAttemptResultQuery query,
        CancellationToken cancellationToken)
    {
        Attempt? attempt = await _attemptRepository.GetByIdWithAnswersAsync(
            query.AttemptId,
            cancellationToken);

        if (attempt is null)
            return Result.Failure<AttemptResultResponse>(
                AttemptErrors.NotFound);

        List<AttemptAnswerResponse> answers = attempt.Answers
            .Select(x => new AttemptAnswerResponse(
                x.QuestionId,
                x.IsCorrect,
                x.TextAnswer,
                x.SelectedOptionIds))
            .ToList();

        return Result.Success(
            new AttemptResultResponse(
                attempt.Id,
                attempt.LessonId,
                attempt.Score,
                attempt.StartedAt,
                attempt.CompletedAt,
                attempt.Status.ToString(),
                answers));
    }
}