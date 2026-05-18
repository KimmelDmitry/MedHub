using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Questions.UpdateQuestionText;

internal sealed class UpdateQuestionTextCommandHandler
    : ICommandHandler<UpdateQuestionTextCommand>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuestionTextCommandHandler(
        IQuestionRepository questionRepository,
        IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateQuestionTextCommand command,
        CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(
            command.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure(QuestionErrors.NotFound);
        }

        var result = question.UpdateText(command.Text);

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result);
    }
}