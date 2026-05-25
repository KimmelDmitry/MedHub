import { Link } from 'react-router-dom';
import { BookOpen, CheckCircle2, Film, ListChecks } from 'lucide-react';
import type { CatalogCourseListItem } from '../api/studentCatalogApi';
import { useEnrollInCourse } from '../hooks/useEnrollInCourse';

type CatalogCourseCardProps = {
  course: CatalogCourseListItem;
};

export function CatalogCourseCard({ course }: CatalogCourseCardProps) {
  const enrollMutation = useEnrollInCourse(course.id);
  const isEnrolled = course.isEnrolled || course.enrollmentStatus?.toLowerCase() === 'active';

  return (
    <article className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm transition hover:border-slate-300 hover:shadow-md">
      <div className="flex flex-col gap-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <h2 className="text-xl font-semibold text-slate-950">{course.title}</h2>
            <p className="mt-2 line-clamp-3 text-sm leading-6 text-slate-600">
              {course.description || 'Описание курса появится позже.'}
            </p>
          </div>
          {isEnrolled && (
            <span className="inline-flex w-fit rounded-md bg-emerald-50 px-2.5 py-1 text-xs font-semibold text-emerald-700">
              Вы записаны
            </span>
          )}
        </div>

        <dl className="grid gap-3 text-sm text-slate-600 sm:grid-cols-3">
          <div className="rounded-lg bg-slate-50 p-3">
            <dt className="flex items-center gap-2 font-medium text-slate-500">
              <BookOpen className="h-4 w-4" />
              Уроки
            </dt>
            <dd className="mt-1 font-semibold text-slate-950">
              {course.publishedLessonsCount}/{course.lessonsCount}
            </dd>
          </div>
          <div className="rounded-lg bg-slate-50 p-3">
            <dt className="flex items-center gap-2 font-medium text-slate-500">
              <ListChecks className="h-4 w-4" />
              Проверки
            </dt>
            <dd className="mt-1 font-semibold text-slate-950">{course.checkpointsCount}</dd>
          </div>
          <div className="rounded-lg bg-slate-50 p-3">
            <dt className="flex items-center gap-2 font-medium text-slate-500">
              {course.hasVideo ? <CheckCircle2 className="h-4 w-4" /> : <Film className="h-4 w-4" />}
              Видео
            </dt>
            <dd className="mt-1 font-semibold text-slate-950">
              {course.hasVideo ? 'готово' : 'нет'}
            </dd>
          </div>
        </dl>

        <div className="flex flex-wrap gap-2">
          {isEnrolled ? (
            <Link
              to={`/catalog/courses/${course.id}`}
              className="inline-flex w-fit items-center justify-center rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700"
            >
              Открыть курс
            </Link>
          ) : (
            <button
              type="button"
              disabled={enrollMutation.isPending}
              onClick={() => enrollMutation.mutate()}
              className="inline-flex w-fit items-center justify-center rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
            >
              {enrollMutation.isPending ? 'Записываем...' : 'Записаться'}
            </button>
          )}

          <Link
            to={`/catalog/courses/${course.id}`}
            className="inline-flex w-fit items-center justify-center rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-700 transition hover:bg-slate-50"
          >
            Подробнее
          </Link>
        </div>

        {enrollMutation.isError && (
          <p className="text-sm text-rose-700">
            Не удалось записаться на курс. Попробуйте еще раз.
          </p>
        )}
      </div>
    </article>
  );
}
