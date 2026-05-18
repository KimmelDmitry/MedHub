using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Questions.CreateQuestion;

internal sealed class CreateQuestionCommandHandler
    : ICommandHandler<CreateQuestionCommand, Guid>
{
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateQuestionCommandHandler(
        ICheckpointRepository checkpointRepository,
        IUnitOfWork unitOfWork)
    {
        _checkpointRepository = checkpointRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var checkpoint = await _checkpointRepository.GetByIdAsync(
            command.CheckpointId,
            cancellationToken);

        if (checkpoint is null)
        {
            return Result.Failure<Guid>(CheckpointErrors.NotFound);
        }
/*
        var questionResult = Question.Create(
            checkpoint.Id,
            command.Text,
            command.Type,
            command.AllowRetry,
            command.TimeLimitSeconds,
            command.RevealCorrectAnswer,
            command.CorrectTextAnswer);

        if (questionResult.IsFailure)
        {
            return Result.Failure<Guid>(questionResult.Error);
        }
*/
        var checkpointResult = checkpoint.AddQuestion(
            text: command.Text,
            type: command.Type,
            allowRetry: command.AllowRetry,
            timeLimitSeconds: command.TimeLimitSeconds,
            revealCorrectAnswer: command.RevealCorrectAnswer);

        if (checkpointResult.IsFailure)
        {
            return Result.Failure<Guid>(checkpointResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(checkpointResult.Value.Id);
    }
}