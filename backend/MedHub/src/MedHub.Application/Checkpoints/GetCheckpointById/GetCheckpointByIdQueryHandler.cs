using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Checkpoints.GetCheckpointById;

internal sealed class GetCheckpointByIdQueryHandler
    : IQueryHandler<GetCheckpointByIdQuery, CheckpointResponse>
{
    private readonly ICheckpointRepository _checkpointRepository;

    public GetCheckpointByIdQueryHandler(ICheckpointRepository checkpointRepository)
    {
        _checkpointRepository = checkpointRepository;
    }

    public async Task<Result<CheckpointResponse>> Handle(
        GetCheckpointByIdQuery query,
        CancellationToken cancellationToken)
    {
        var checkpoint = await _checkpointRepository.GetByIdAsync(
            query.CheckpointId,
            cancellationToken);

        if (checkpoint is null)
        {
            return Result.Failure<CheckpointResponse>(CheckpointErrors.NotFound);
        }

        var response = new CheckpointResponse(
            checkpoint.Id,
            checkpoint.VideoId,
            checkpoint.Timestamp.Value,
            checkpoint.OrderNumber,
            checkpoint.Title,
            checkpoint.IsRequired,
            checkpoint.IsGraded,
            checkpoint.Status.ToString(),
            checkpoint.Questions
                .OrderBy(x => x.Id)
                .Select(x => new CheckpointQuestionResponse(
                    x.Id,
                    x.Text.Value,
                    x.Type.ToString(),
                    x.AllowRetry,
                    x.TimeLimitSeconds,
                    x.RevealCorrectAnswer,
                    x.CorrectTextAnswer,
                    x.AnswerOptions
                        .Select(o => new CheckpointAnswerOptionResponse(
                            o.Id,
                            o.Text.Value,
                            o.IsCorrect))
                        .ToList()))
                .ToList());

        return Result.Success(response);
    }
}