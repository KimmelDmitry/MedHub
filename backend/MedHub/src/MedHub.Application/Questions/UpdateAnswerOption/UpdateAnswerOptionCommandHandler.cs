using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Questions.UpdateAnswerOption;

internal sealed class UpdateAnswerOptionCommandHandler
    : ICommandHandler<UpdateAnswerOptionCommand>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAnswerOptionCommandHandler(
        IQuestionRepository questionRepository,
        IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateAnswerOptionCommand command,
        CancellationToken cancellationToken)
    {
        Question? question = await _questionRepository.GetByIdAsync(
            command.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure(QuestionErrors.NotFound);
        }

        var result = question.UpdateAnswerOption(
            command.AnswerOptionId,
            command.Text,
            command.IsCorrect);

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}