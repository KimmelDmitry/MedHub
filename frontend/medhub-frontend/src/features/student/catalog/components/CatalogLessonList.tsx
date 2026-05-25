import { Link } from 'react-router-dom';
import { Clock3, Film, ListChecks } from 'lucide-react';
import type { CatalogLessonItem } from '../api/studentCatalogApi';

type CatalogLessonListProps = {
  lessons: CatalogLessonItem[];
  isEnrolled: boolean;
};

function formatDuration(seconds?: number | null) {
  if (!seconds || seconds <= 0) {
    return 'видео';
  }

  return `${Math.ceil(seconds / 60)} мин`;
}

export function CatalogLessonList({ lessons, isEnrolled }: CatalogLessonListProps) {
  if (lessons.length === 0) {
    return (
      <div className="rounded-lg border border-dashed border-slate-300 bg-slate-50 p-6 text-sm text-slate-600">
        В этом курсе пока нет опубликованных уроков.
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {lessons.map((lesson) => (
        <article
          key={lesson.id}
          className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm"
        >
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div className="min-w-0">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">
                Урок {lesson.order}
              </p>
              <h2 className="mt-2 text-lg font-semibold text-slate-950">{lesson.title}</h2>
              <div className="mt-3 flex flex-wrap gap-2 text-xs font-medium text-slate-600">
                <span className="inline-flex items-center gap-1.5 rounded-md bg-slate-100 px-2 py-1">
                  <Film className="h-3.5 w-3.5" />
                  {lesson.videoReady ? 'видео готово' : lesson.hasVideo ? 'обрабатывается' : 'без видео'}
                </span>
                <span className="inline-flex items-center gap-1.5 rounded-md bg-slate-100 px-2 py-1">
                  <Clock3 className="h-3.5 w-3.5" />
                  {formatDuration(lesson.durationSeconds)}
                </span>
                <span className="inline-flex items-center gap-1.5 rounded-md bg-slate-100 px-2 py-1">
                  <ListChecks className="h-3.5 w-3.5" />
                  {lesson.checkpointsCount} проверок
                </span>
                <span className="rounded-md bg-slate-100 px-2 py-1">{lesson.contentType}</span>
              </div>
            </div>

            {isEnrolled && lesson.videoReady ? (
              <Link
                to={`/student/lessons/${lesson.id}`}
                className="inline-flex items-center justify-center rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700"
              >
                Начать урок
              </Link>
            ) : (
              <button
                type="button"
                disabled
                className="inline-flex cursor-not-allowed items-center justify-center rounded-lg bg-slate-200 px-4 py-3 text-sm font-semibold text-slate-500"
              >
                {!isEnrolled ? 'Запишитесь на курс' : 'Видео обрабатывается'}
              </button>
            )}
          </div>
        </article>
      ))}
    </div>
  );
}
