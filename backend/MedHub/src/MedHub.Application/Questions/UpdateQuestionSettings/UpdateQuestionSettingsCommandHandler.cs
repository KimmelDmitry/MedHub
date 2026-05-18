using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Questions.UpdateQuestionSettings;

internal sealed class UpdateQuestionSettingsCommandHandler
    : ICommandHandler<UpdateQuestionSettingsCommand>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuestionSettingsCommandHandler(
        IQuestionRepository questionRepository,
        IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateQuestionSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(
            command.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure(QuestionErrors.NotFound);
        }

        var result = question.UpdateSettings(
            command.AllowRetry,
            command.TimeLimitSeconds,
            command.RevealCorrectAnswer,
            command.CorrectTextAnswer);

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}