import { CheckCircle2, Clock3, EyeOff, XCircle } from 'lucide-react';
import type { AttemptAnswerReview } from '../api/studentRuntimeApi';
import { formatSeconds } from '../utils/timeFormat';

type StudentAnswerReviewListProps = {
  answers: AttemptAnswerReview[];
};

function getCheckpointTitle(answer: AttemptAnswerReview) {
  return answer.checkpointTitle?.trim() || 'Checkpoint';
}

function renderOptionList(options: { id: string; text: string }[]) {
  if (options.length === 0) {
    return <span className="text-slate-500">Ответ не выбран</span>;
  }

  return options.map((option) => option.text).join(', ');
}

function renderCorrectAnswer(answer: AttemptAnswerReview) {
  if (answer.requiresManualReview) {
    return 'Ответ требует ручной проверки.';
  }

  if (answer.isCorrect) {
    return 'Ваш ответ верный.';
  }

  if (answer.revealCorrectAnswer) {
    return renderOptionList(answer.correctOptions);
  }

  return (
    <span className="inline-flex items-center gap-1.5 text-slate-600">
      <EyeOff className="h-4 w-4" />
      Правильный ответ скрыт преподавателем
    </span>
  );
}

export function StudentAnswerReviewList({ answers }: StudentAnswerReviewListProps) {
  if (answers.length === 0) {
    return (
      <div className="rounded-lg border border-dashed border-slate-300 bg-slate-50 p-5 text-sm text-slate-600">
        Для этой попытки пока нет сохраненных ответов.
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {answers.map((answer) => (
        <article
          key={`${answer.checkpointId}-${answer.questionId}`}
          className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm"
        >
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <div className="flex flex-wrap items-center gap-2">
                <span className="inline-flex items-center gap-1.5 rounded-md bg-slate-100 px-2 py-1 text-xs font-semibold text-slate-600">
                  <Clock3 className="h-3.5 w-3.5" />
                  {formatSeconds(answer.timestampSeconds)}
                </span>
                <span className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                  {getCheckpointTitle(answer)}
                </span>
              </div>
              <h3 className="mt-3 text-base font-semibold text-slate-950">
                {answer.questionText}
              </h3>
            </div>

            <span
              className={`inline-flex items-center gap-1.5 rounded-md px-2.5 py-1 text-xs font-semibold ${
                answer.isCorrect
                  ? 'bg-emerald-50 text-emerald-700'
                  : 'bg-rose-50 text-rose-700'
              }`}
            >
              {answer.isCorrect ? (
                <CheckCircle2 className="h-3.5 w-3.5" />
              ) : (
                <XCircle className="h-3.5 w-3.5" />
              )}
              {answer.isCorrect ? 'Верно' : 'Неверно'}
            </span>
          </div>

          <dl className="mt-5 grid gap-4 text-sm sm:grid-cols-2">
            <div className="rounded-lg bg-slate-50 p-4">
              <dt className="font-medium text-slate-500">Ваш ответ</dt>
              <dd className="mt-2 font-semibold text-slate-950">
                {answer.textAnswer?.trim() || renderOptionList(answer.selectedOptions)}
              </dd>
            </div>

            <div className="rounded-lg bg-slate-50 p-4">
              <dt className="font-medium text-slate-500">
                {answer.isCorrect ? 'Итог' : 'Правильный ответ'}
              </dt>
              <dd className="mt-2 font-semibold text-slate-950">
                {renderCorrectAnswer(answer)}
              </dd>
            </div>
          </dl>

          {answer.requiresManualReview && (
            <p className="mt-4 rounded-lg bg-amber-50 px-4 py-3 text-sm font-medium text-amber-800">
              Ответ требует ручной проверки.
            </p>
          )}
        </article>
      ))}
    </div>
  );
}
