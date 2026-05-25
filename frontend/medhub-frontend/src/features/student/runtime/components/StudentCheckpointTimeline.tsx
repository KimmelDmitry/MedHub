import { CheckCircle2, EyeOff } from 'lucide-react';
import type { StudentRuntimeCheckpoint } from '../api/studentRuntimeApi';
import { formatSeconds } from '../utils/timeFormat';

type StudentCheckpointTimelineProps = {
  checkpoints: StudentRuntimeCheckpoint[];
  answeredQuestionIds: Set<string>;
};

function getCheckpointTitle(checkpoint: StudentRuntimeCheckpoint) {
  return checkpoint.title?.trim() || 'Checkpoint';
}

function isPublishedCheckpoint(checkpoint: StudentRuntimeCheckpoint) {
  return !checkpoint.status || checkpoint.status.toLowerCase() === 'published';
}

function getSupportedQuestionIds(checkpoint: StudentRuntimeCheckpoint) {
  return checkpoint.questions
    .filter((question) => {
      const type = String(question.type).trim().toLowerCase();
      return type === 'singlechoice' || type === '1';
    })
    .map((question) => question.questionId);
}

export function StudentCheckpointTimeline({
  checkpoints,
  answeredQuestionIds,
}: StudentCheckpointTimelineProps) {
  const answeredCheckpoints = checkpoints
    .filter(isPublishedCheckpoint)
    .map((checkpoint) => {
      const supportedQuestionIds = getSupportedQuestionIds(checkpoint);
      const answeredCount = supportedQuestionIds.filter((questionId) =>
        answeredQuestionIds.has(questionId),
      ).length;

      return {
        checkpoint,
        answeredCount,
        supportedCount: supportedQuestionIds.length,
      };
    })
    .filter((item) => item.answeredCount > 0)
    .sort(
      (left, right) =>
        left.checkpoint.timestampSeconds - right.checkpoint.timestampSeconds ||
        left.checkpoint.orderNumber - right.checkpoint.orderNumber,
    );

  return (
    <section className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-slate-950">Интерактивные проверки</h2>
          <p className="mt-1 text-sm leading-6 text-slate-600">
            Будущие checkpoints скрыты. Видео остановится само, когда нужно ответить на вопрос.
          </p>
        </div>
        <span className="inline-flex w-fit items-center gap-1.5 rounded-md bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">
          <EyeOff className="h-3.5 w-3.5" />
          без подсказок
        </span>
      </div>

      {answeredCheckpoints.length === 0 ? (
        <div className="mt-5 rounded-lg border border-dashed border-slate-300 bg-slate-50 p-5 text-sm leading-6 text-slate-600">
          Пройденные checkpoints появятся здесь после ответа. Точные точки остановки заранее не показываются.
        </div>
      ) : (
        <div className="mt-5 space-y-3">
          {answeredCheckpoints.map(({ checkpoint, answeredCount, supportedCount }) => (
            <article
              key={checkpoint.checkpointId}
              className="rounded-lg border border-emerald-100 bg-emerald-50/70 p-4 text-slate-700"
            >
              <div className="flex flex-wrap items-center justify-between gap-2">
                <span className="inline-flex items-center gap-1.5 rounded-md bg-white px-2 py-1 text-xs font-semibold text-emerald-700">
                  <CheckCircle2 className="h-3.5 w-3.5" />
                  {formatSeconds(checkpoint.timestampSeconds)}
                </span>
                <span className="text-xs font-medium text-emerald-700">
                  {answeredCount}/{supportedCount} answered
                </span>
              </div>
              <p className="mt-3 text-sm font-semibold text-slate-950">
                {getCheckpointTitle(checkpoint)}
              </p>
              <p className="mt-2 text-xs text-slate-500">
                {checkpoint.isRequired ? 'Required' : 'Optional'} ·{' '}
                {checkpoint.isGraded ? 'Graded' : 'Practice'}
              </p>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
