using System.Data;
using Dapper;
using MedHub.Application.Abstractions.Authentication;
using MedHub.Application.Abstractions.Data;
using MedHub.Application.Abstractions.Messaging;
using MedHub.Domain.Abstractions;
using MedHub.Domain.Attempts;
using MedHub.Domain.Checkpoints;
using MedHub.Domain.Checkpoints.Enums;
using MedHub.Domain.Checkpoints.Errors;

namespace MedHub.Application.Attempts.GetAttemptResult;

internal sealed class GetAttemptResultQueryHandler
    : IQueryHandler<GetAttemptResultQuery, AttemptResultResponse>
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IUserContext _userContext;

    public GetAttemptResultQueryHandler(
        IAttemptRepository attemptRepository,
        ISqlConnectionFactory sqlConnectionFactory,
        IUserContext userContext)
    {
        _attemptRepository = attemptRepository;
        _sqlConnectionFactory = sqlConnectionFactory;
        _userContext = userContext;
    }

    public async Task<Result<AttemptResultResponse>> Handle(
        GetAttemptResultQuery query,
        CancellationToken cancellationToken)
    {
        Attempt? attempt = await _attemptRepository.GetByIdWithAnswersAsync(
            query.AttemptId,
            cancellationToken);

        if (attempt is null)
        {
            return Result.Failure<AttemptResultResponse>(
                AttemptErrors.NotFound);
        }

        if (attempt.StudentId != _userContext.UserId)
        {
            return Result.Failure<AttemptResultResponse>(
                new Error(
                    "Attempt.Forbidden",
                    "Only the attempt owner can read the attempt result."));
        }

        if (attempt.Status != AttemptStatus.Completed)
        {
            return Result.Failure<AttemptResultResponse>(
                AttemptErrors.NotCompleted);
        }

        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        const string answersSql = """
            SELECT
                aa.question_id AS QuestionId,
                aa.selected_option_ids AS SelectedOptionIds,
                aa.text_answer AS TextAnswer,
                aa.is_correct AS IsCorrect,
                aa.requires_manual_review AS RequiresManualReview,
                cp.id AS CheckpointId,
                cp.title AS CheckpointTitle,
                cp.timestamp_seconds AS TimestampSeconds,
                q.text AS QuestionText,
                q.type AS Type,
                q.reveal_correct_answer AS RevealCorrectAnswer
            FROM attempt_answers aa
            INNER JOIN checkpoint_questions q ON q.id = aa.question_id
            INNER JOIN checkpoints cp ON cp.id = q.checkpoint_id
            WHERE aa.attempt_id = @AttemptId
            ORDER BY cp.timestamp_seconds, cp.order_number, q.id
            """;

        List<AttemptAnswerReviewRow> answerRows = (await connection.QueryAsync<AttemptAnswerReviewRow>(
            new CommandDefinition(
                answersSql,
                new { query.AttemptId },
                cancellationToken: cancellationToken))).ToList();

        Dictionary<Guid, List<AnswerOptionReviewRow>> optionsByQuestion = new();

        Guid[] questionIds = answerRows
            .Select(x => x.QuestionId)
            .Distinct()
            .ToArray();

        if (questionIds.Length > 0)
        {
            const string optionsSql = """
                SELECT
                    id AS Id,
                    question_id AS QuestionId,
                    text AS Text,
                    is_correct AS IsCorrect
                FROM checkpoint_answer_options
                WHERE question_id = ANY(@QuestionIds)
                ORDER BY id
                """;

            List<AnswerOptionReviewRow> optionRows = (await connection.QueryAsync<AnswerOptionReviewRow>(
                new CommandDefinition(
                    optionsSql,
                    new { QuestionIds = questionIds },
                    cancellationToken: cancellationToken))).ToList();

            optionsByQuestion = optionRows
                .GroupBy(x => x.QuestionId)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        List<AttemptAnswerReviewResponse> answers = answerRows
            .Select(answer =>
            {
                optionsByQuestion.TryGetValue(
                    answer.QuestionId,
                    out List<AnswerOptionReviewRow>? options);

                var selectedOptionIds = answer.SelectedOptionIds.ToHashSet();

                List<AnswerOptionReviewResponse> selectedOptions = (options ?? [])
                    .Where(option => selectedOptionIds.Contains(option.Id))
                    .Select(option => new AnswerOptionReviewResponse(
                        option.Id,
                        option.Text))
                    .ToList();

                List<AnswerOptionReviewResponse> correctOptions = answer.RevealCorrectAnswer
                    ? (options ?? [])
                        .Where(option => option.IsCorrect)
                        .Select(option => new AnswerOptionReviewResponse(
                            option.Id,
                            option.Text))
                        .ToList()
                    : [];

                return new AttemptAnswerReviewResponse(
                    answer.QuestionId,
                    answer.CheckpointId,
                    answer.CheckpointTitle,
                    answer.TimestampSeconds,
                    answer.QuestionText,
                    answer.Type,
                    selectedOptions,
                    answer.IsCorrect,
                    answer.RevealCorrectAnswer,
                    correctOptions,
                    answer.TextAnswer,
                    answer.RequiresManualReview);
            })
            .ToList();

        return Result.Success(
            new AttemptResultResponse(
                attempt.Id,
                attempt.LessonId,
                attempt.Status.ToString(),
                attempt.Score,
                attempt.StartedAt,
                attempt.CompletedAt,
                answers.Count,
                answers.Count(x => x.IsCorrect),
                answers));
    }

    private sealed class AttemptAnswerReviewRow
    {
        public Guid QuestionId { get; init; }
        public Guid[] SelectedOptionIds { get; init; } = [];
        public string? TextAnswer { get; init; }
        public bool IsCorrect { get; init; }
        public bool RequiresManualReview { get; init; }
        public Guid CheckpointId { get; init; }
        public string? CheckpointTitle { get; init; }
        public int TimestampSeconds { get; init; }
        public string QuestionText { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public bool RevealCorrectAnswer { get; init; }
    }

    private sealed class AnswerOptionReviewRow
    {
        public Guid Id { get; init; }
        public Guid QuestionId { get; init; }
        public string Text { get; init; } = string.Empty;
        public bool IsCorrect { get; init; }
    }
}
