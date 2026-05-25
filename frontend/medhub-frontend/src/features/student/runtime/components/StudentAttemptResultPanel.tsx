import { Trophy } from 'lucide-react';
import type { AttemptResultResponse } from '../api/studentRuntimeApi';
import { StudentAnswerReviewList } from './StudentAnswerReviewList';

type StudentAttemptResultPanelProps = {
  result: AttemptResultResponse;
};

function formatDate(value?: string | null) {
  if (!value) {
    return '-';
  }

  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

export function StudentAttemptResultPanel({ result }: StudentAttemptResultPanelProps) {
  return (
    <section className="space-y-5">
      <div className="rounded-lg border border-emerald-200 bg-emerald-50 p-5 text-emerald-950">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div className="flex items-start gap-3">
            <Trophy className="mt-1 h-5 w-5" />
            <div>
              <h2 className="text-lg font-semibold">Урок завершен</h2>
              <p className="mt-1 text-sm">
                Итоговый результат: {result.score}% · статус {result.status}
              </p>
            </div>
          </div>

          <dl className="grid grid-cols-2 gap-3 text-sm sm:min-w-72">
            <div className="rounded-lg bg-white/70 p-3">
              <dt className="font-medium text-emerald-700">Верных ответов</dt>
              <dd className="mt-1 text-xl font-semibold">
                {result.correctAnswers}/{result.totalQuestions}
              </dd>
            </div>
            <div className="rounded-lg bg-white/70 p-3">
              <dt className="font-medium text-emerald-700">Завершено</dt>
              <dd className="mt-1 text-sm font-semibold">
                {formatDate(result.completedAt)}
              </dd>
            </div>
          </dl>
        </div>
      </div>

      <div>
        <h2 className="text-lg font-semibold text-slate-950">Разбор ответов</h2>
        <p className="mt-1 text-sm leading-6 text-slate-600">
          Правильные ответы показываются только там, где преподаватель разрешил раскрытие после завершения попытки.
        </p>
      </div>

      <StudentAnswerReviewList answers={result.answers} />
    </section>
  );
}
