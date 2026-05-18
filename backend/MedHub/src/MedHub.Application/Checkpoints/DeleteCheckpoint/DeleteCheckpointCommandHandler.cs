using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Enums;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Checkpoints.DeleteCheckpoint;

internal sealed class DeleteCheckpointCommandHandler
    : ICommandHandler<DeleteCheckpointCommand>
{
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCheckpointCommandHandler(
        ICheckpointRepository checkpointRepository,
        IUnitOfWork unitOfWork)
    {
        _checkpointRepository = checkpointRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteCheckpointCommand command,
        CancellationToken cancellationToken)
    {
        var checkpoint = await _checkpointRepository.GetByIdAsync(
            command.CheckpointId,
            cancellationToken);

        if (checkpoint is null)
        {
            return Result.Failure(CheckpointErrors.NotFound);
        }

        if (checkpoint.Status == CheckpointStatus.Published)
        {
            return Result.Failure(
                new Error(
                    "Checkpoint.CannotDeletePublished",
                    "Опубликованную контрольную точку нельзя удалить"));
        }

        _checkpointRepository.Remove(checkpoint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}