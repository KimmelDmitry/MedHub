using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Questions.GetQuestionById;

internal sealed class GetQuestionByIdQueryHandler
    : IQueryHandler<GetQuestionByIdQuery, QuestionResponse>
{
    private readonly IQuestionRepository _questionRepository;

    public GetQuestionByIdQueryHandler(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    public async Task<Result<QuestionResponse>> Handle(
        GetQuestionByIdQuery query,
        CancellationToken cancellationToken)
    {
        var question = await _questionRepository.GetByIdAsync(
            query.QuestionId,
            cancellationToken);

        if (question is null)
        {
            return Result.Failure<QuestionResponse>(QuestionErrors.NotFound);
        }

        var response = new QuestionResponse(
            question.Id,
            question.CheckpointId,
            question.Text.Value,
            question.Type,
            question.AllowRetry,
            question.TimeLimitSeconds,
            question.RevealCorrectAnswer,
            question.CorrectTextAnswer,
            question.AnswerOptions
                .Select(x => new AnswerOptionResponse(
                    x.Id,
                    x.Text.Value,
                    x.IsCorrect))
                .ToList());

        return Result.Success(response);
    }
}