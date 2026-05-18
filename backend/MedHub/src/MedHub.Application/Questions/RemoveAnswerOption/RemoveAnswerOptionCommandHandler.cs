using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Questions.RemoveAnswerOption;

internal sealed class RemoveAnswerOptionCommandHandler
    : ICommandHandler<RemoveAnswerOptionCommand>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveAnswerOptionCommandHandler(
        IQuestionRepository questionRepository,
        IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RemoveAnswerOptionCommand command,
        CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(
            command.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure(QuestionErrors.NotFound);
        }

        var result = question.RemoveOption(command.AnswerOptionId);

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}