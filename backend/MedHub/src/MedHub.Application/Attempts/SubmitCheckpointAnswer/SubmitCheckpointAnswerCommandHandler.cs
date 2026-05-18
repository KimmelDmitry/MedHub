using MedHub.Application.Abstractions.Clock;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Attempts;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Attempts.SubmitCheckpointAnswer;

internal sealed class SubmitCheckpointAnswerCommandHandler
    : ICommandHandler<SubmitCheckpointAnswerCommand, SubmitCheckpointAnswerResponse>
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SubmitCheckpointAnswerCommandHandler(
        IAttemptRepository attemptRepository,
        IQuestionRepository questionRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _attemptRepository = attemptRepository;
        _questionRepository = questionRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<SubmitCheckpointAnswerResponse>> Handle(
        SubmitCheckpointAnswerCommand command,
        CancellationToken cancellationToken)
    {
        Attempt? attempt = await _attemptRepository.GetByIdAsync(
            command.AttemptId,
            cancellationToken);

        if (attempt is null)
            return Result.Failure<SubmitCheckpointAnswerResponse>(
                AttemptErrors.NotFound);

        Question? question = await _questionRepository.GetByIdAsync(
            command.QuestionId,
            cancellationToken);

        if (question is null)
            return Result.Failure<SubmitCheckpointAnswerResponse>(
                QuestionErrors.NotFound);
        
        DateTime nowUtc = _dateTimeProvider.UtcNow;

        Result<Answer> result = attempt.SubmitAnswer(
            question,
            command.SelectedOptionIds,
            command.TextAnswer,
            nowUtc);

        if (result.IsFailure)
            return Result.Failure<SubmitCheckpointAnswerResponse>(
                result.Error);

        Answer answer = result.Value;

        return Result.Success(
            new SubmitCheckpointAnswerResponse(
                answer.Id,
                answer.IsCorrect,
                attempt.Score));
    }
}