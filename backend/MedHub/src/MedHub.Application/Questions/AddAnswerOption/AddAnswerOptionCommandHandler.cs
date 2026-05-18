using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Questions.AddAnswerOption;

internal sealed class AddAnswerOptionCommandHandler
    : ICommandHandler<AddAnswerOptionCommand, Guid>
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddAnswerOptionCommandHandler(
        IQuestionRepository questionRepository,
        IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        AddAnswerOptionCommand command,
        CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(
            command.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure<Guid>(QuestionErrors.NotFound);
        }

        var result = question.AddOption(command.Text, command.IsCorrect);
        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}