import { Link, Navigate, useParams } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { CatalogLessonList } from '../components/CatalogLessonList';
import { useCatalogCourse } from '../hooks/useCatalogCourse';
import { useEnrollInCourse } from '../hooks/useEnrollInCourse';

export function StudentCatalogCoursePage() {
  const { courseId } = useParams<{ courseId: string }>();
  const courseQuery = useCatalogCourse(courseId);
  const enrollMutation = useEnrollInCourse(courseId);
  const course = courseQuery.data;
  const isEnrolled = Boolean(course?.isEnrolled || course?.enrollmentStatus?.toLowerCase() === 'active');

  if (!courseId) {
    return <Navigate to="/catalog" replace />;
  }

  return (
    <section className="mx-auto max-w-5xl space-y-6">
      <header className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <Link
          to="/catalog"
          className="inline-flex items-center gap-2 text-sm font-semibold text-slate-600 transition hover:text-slate-950"
        >
          <ArrowLeft className="h-4 w-4" />
          В каталог
        </Link>

        {courseQuery.isLoading && (
          <div className="mt-5 text-sm text-slate-600">Загружаем курс...</div>
        )}

        {courseQuery.isError && (
          <div className="mt-5 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            Курс не найден или еще не опубликован.
          </div>
        )}

        {course && (
          <>
            <div className="mt-4 flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
              <div>
                <h1 className="text-3xl font-semibold text-slate-950">{course.title}</h1>
                <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
                  {course.description || 'Описание курса появится позже.'}
                </p>
              </div>

              {isEnrolled ? (
                <span className="inline-flex w-fit rounded-md bg-emerald-50 px-3 py-2 text-sm font-semibold text-emerald-700">
                  Вы записаны на курс
                </span>
              ) : (
                <button
                  type="button"
                  disabled={enrollMutation.isPending}
                  onClick={() => enrollMutation.mutate()}
                  className="inline-flex w-fit rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
                >
                  {enrollMutation.isPending ? 'Записываем...' : 'Записаться на курс'}
                </button>
              )}
            </div>

            {enrollMutation.isError && (
              <div className="mt-4 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                Не удалось записаться на курс. Попробуйте еще раз.
              </div>
            )}

            {!isEnrolled && (
              <div className="mt-4 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
                Уроки можно посмотреть в списке, но запуск видео доступен только после записи на курс.
              </div>
            )}
          </>
        )}
      </header>

      {course && (
        <section className="space-y-4">
          <div className="flex flex-col gap-1">
            <h2 className="text-xl font-semibold text-slate-950">Уроки</h2>
            <p className="text-sm text-slate-600">
              В списке отображаются опубликованные уроки курса.
            </p>
          </div>
          <CatalogLessonList lessons={course.lessons} isEnrolled={isEnrolled} />
        </section>
      )}
    </section>
  );
}
