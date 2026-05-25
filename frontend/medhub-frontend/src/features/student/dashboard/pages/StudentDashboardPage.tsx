import { Link } from 'react-router-dom';
import { ArrowRight, BookOpen, Search } from 'lucide-react';
import { useAuth } from '../../../auth/hooks/useAuth';
import { StudentDashboardCourseCard } from '../components/StudentDashboardCourseCard';
import { StudentRecentAttemptsList } from '../components/StudentRecentAttemptsList';
import { useStudentDashboard } from '../hooks/useStudentDashboard';

export function StudentDashboardPage() {
  const { user } = useAuth();
  const dashboardQuery = useStudentDashboard();
  const dashboard = dashboardQuery.data;
  const displayName = user?.firstName || user?.email || 'студент';

  return (
    <section className="mx-auto max-w-6xl space-y-6">
      <header className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.2em] text-slate-500">
              Кабинет студента
            </p>
            <h1 className="mt-3 text-3xl font-semibold text-slate-950">
              Продолжайте обучение, {displayName}
            </h1>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
              Здесь собраны ваши курсы, прогресс по урокам и последние попытки.
            </p>
          </div>

          <Link
            to="/catalog"
            className="inline-flex items-center justify-center gap-2 rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700"
          >
            Каталог курсов
            <Search className="h-4 w-4" />
          </Link>
        </div>
      </header>

      {dashboardQuery.isLoading && (
        <div className="rounded-lg border border-slate-200 bg-white p-6 text-sm text-slate-600 shadow-sm">
          Загружаем прогресс...
        </div>
      )}

      {dashboardQuery.isError && (
        <div className="rounded-lg border border-rose-200 bg-rose-50 p-6 text-sm text-rose-700">
          Не удалось загрузить кабинет студента. Обновите страницу или повторите вход.
        </div>
      )}

      {dashboard && dashboard.enrolledCourses.length === 0 && (
        <section className="rounded-lg border border-dashed border-slate-300 bg-white p-8 text-center shadow-sm">
          <BookOpen className="mx-auto h-10 w-10 text-slate-400" />
          <h2 className="mt-4 text-xl font-semibold text-slate-950">
            Вы пока не записаны на курсы
          </h2>
          <p className="mx-auto mt-2 max-w-xl text-sm leading-6 text-slate-600">
            Перейдите в каталог, выберите опубликованный курс и начните обучение.
          </p>
          <Link
            to="/catalog"
            className="mt-5 inline-flex items-center justify-center gap-2 rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700"
          >
            Перейти в каталог
            <ArrowRight className="h-4 w-4" />
          </Link>
        </section>
      )}

      {dashboard && dashboard.enrolledCourses.length > 0 && (
        <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_380px] xl:items-start">
          <section className="space-y-4">
            <div>
              <h2 className="text-xl font-semibold text-slate-950">Мои курсы</h2>
              <p className="mt-1 text-sm text-slate-600">
                Прогресс считается по опубликованным урокам курса.
              </p>
            </div>

            {dashboard.enrolledCourses.map((course) => (
              <StudentDashboardCourseCard
                key={course.enrollmentId}
                course={course}
              />
            ))}
          </section>

          <aside className="space-y-4 xl:sticky xl:top-6">
            <div>
              <h2 className="text-xl font-semibold text-slate-950">Последние попытки</h2>
              <p className="mt-1 text-sm text-slate-600">
                Быстрый переход к разбору или продолжению урока.
              </p>
            </div>

            <StudentRecentAttemptsList attempts={dashboard.recentAttempts} />
          </aside>
        </div>
      )}
    </section>
  );
}
