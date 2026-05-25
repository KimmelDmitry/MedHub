using MedHub.Domain.Checkpoints;
using MedHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedHub.Infrastructure.Repositories;

internal sealed class QuestionRepository : Repository<Question>, IQuestionRepository
{
    public QuestionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Question?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Question>()
            .Include(x => x.AnswerOptions)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Question>> GetByCheckpointIdAsync(
        Guid checkpointId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Question>()
            .Include(x => x.AnswerOptions)
            .Where(x => x.CheckpointId == checkpointId)
            .ToListAsync(cancellationToken);
    }

    public void Add(Question question)
    {
        DbContext.Set<Question>().Add(question);
    }

    public void AddAnswerOption(AnswerOption answerOption)
    {
        DbContext.Set<AnswerOption>().Add(answerOption);
    }

    public void Remove(Question question)
    {
        DbContext.Set<Question>().Remove(question);
    }
}
