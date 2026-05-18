using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Questions.DeleteQuestion;

internal sealed class DeleteQuestionCommandHandler
    : ICommandHandler<DeleteQuestionCommand>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ICheckpointRepository _checkpointRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteQuestionCommandHandler(
        IQuestionRepository questionRepository,
        ICheckpointRepository checkpointRepository,
        IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _checkpointRepository = checkpointRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(
            command.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure(QuestionErrors.NotFound);
        }

        var checkpoint = await _checkpointRepository.GetByIdAsync(
            question.CheckpointId,
            cancellationToken);

        if (checkpoint is null)
        {
            return Result.Failure(CheckpointErrors.NotFound);
        }

        var result = checkpoint.RemoveQuestion(question.Id);
        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}