import { Link } from 'react-router-dom';
import { CheckCircle2, Clock3, PlayCircle } from 'lucide-react';
import type { StudentRecentAttempt } from '../api/studentDashboardApi';

type StudentRecentAttemptsListProps = {
  attempts: StudentRecentAttempt[];
};

function normalizeStatus(status: string) {
  return status.trim().toLowerCase();
}

function formatDate(value?: string | null) {
  if (!value) {
    return '';
  }

  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

export function StudentRecentAttemptsList({ attempts }: StudentRecentAttemptsListProps) {
  if (attempts.length === 0) {
    return (
      <div className="rounded-lg border border-dashed border-slate-300 bg-slate-50 p-5 text-sm text-slate-600">
        Завершите первый урок, и здесь появится история прохождения.
      </div>
    );
  }

  return (
    <div className="divide-y divide-slate-200 rounded-lg border border-slate-200 bg-white shadow-sm">
      {attempts.map((attempt) => {
        const status = normalizeStatus(attempt.status);
        const isCompleted = status === 'completed';

        return (
          <article
            key={attempt.attemptId}
            className="flex flex-col gap-4 p-4 sm:flex-row sm:items-center sm:justify-between"
          >
            <div className="min-w-0">
              <div className="flex flex-wrap items-center gap-2">
                <span
                  className={`inline-flex items-center gap-1.5 rounded-md px-2 py-1 text-xs font-semibold ${
                    isCompleted
                      ? 'bg-emerald-50 text-emerald-700'
                      : 'bg-sky-50 text-sky-700'
                  }`}
                >
                  {isCompleted ? (
                    <CheckCircle2 className="h-3.5 w-3.5" />
                  ) : (
                    <PlayCircle className="h-3.5 w-3.5" />
                  )}
                  {isCompleted ? 'Завершен' : 'В процессе'}
                </span>
                <span className="inline-flex items-center gap-1.5 text-xs font-medium text-slate-500">
                  <Clock3 className="h-3.5 w-3.5" />
                  {formatDate(attempt.updatedAtUtc)}
                </span>
              </div>
              <h3 className="mt-2 font-semibold text-slate-950">{attempt.lessonTitle}</h3>
              <p className="mt-1 text-sm text-slate-600">{attempt.courseTitle}</p>
            </div>

            <div className="flex shrink-0 items-center gap-3">
              {isCompleted && (
                <span className="rounded-md bg-slate-100 px-2.5 py-1 text-sm font-semibold text-slate-700">
                  {attempt.score}%
                </span>
              )}
              <Link
                to={`/student/lessons/${attempt.lessonId}`}
                className="inline-flex items-center justify-center rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-700 transition hover:bg-slate-50"
              >
                {isCompleted ? 'Разбор' : 'Продолжить'}
              </Link>
            </div>
          </article>
        );
      })}
    </div>
  );
}
