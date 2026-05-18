using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Checkpoints;

namespace MedHub.Application.Questions.GetCheckpointQuestions;

internal sealed class GetCheckpointQuestionsQueryHandler
    : IQueryHandler<GetCheckpointQuestionsQuery, IReadOnlyList<CheckpointQuestionResponse>>
{
    private readonly IQuestionRepository _questionRepository;

    public GetCheckpointQuestionsQueryHandler(
        IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    public async Task<Result<IReadOnlyList<CheckpointQuestionResponse>>> Handle(
        GetCheckpointQuestionsQuery query,
        CancellationToken cancellationToken)
    {
        var questions = await _questionRepository.GetByCheckpointIdAsync(
            query.CheckpointId,
            cancellationToken);

        var response = questions
            .Select(x => new CheckpointQuestionResponse(
                x.Id,
                x.Text.Value,
                x.Type,
                x.AnswerOptions.Count))
            .ToList();

        return Result.Success<IReadOnlyList<CheckpointQuestionResponse>>(response);
    }
}