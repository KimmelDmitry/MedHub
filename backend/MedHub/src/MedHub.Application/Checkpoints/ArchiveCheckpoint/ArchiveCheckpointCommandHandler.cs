using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Checkpoints.ArchiveCheckpoint;

internal sealed class ArchiveCheckpointCommandHandler
    : ICommandHandler<ArchiveCheckpointCommand>
{
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveCheckpointCommandHandler(
        ICheckpointRepository checkpointRepository,
        IUnitOfWork unitOfWork)
    {
        _checkpointRepository = checkpointRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ArchiveCheckpointCommand command,
        CancellationToken cancellationToken)
    {
        var checkpoint = await _checkpointRepository.GetByIdAsync(
            command.CheckpointId,
            cancellationToken);

        if (checkpoint is null)
        {
            return Result.Failure(CheckpointErrors.NotFound);
        }

        var result = checkpoint.Archive();
        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}