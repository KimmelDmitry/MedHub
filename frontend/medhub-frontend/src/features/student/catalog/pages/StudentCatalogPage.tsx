import { useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowLeft, ChevronLeft, ChevronRight } from 'lucide-react';
import { CatalogCourseList } from '../components/CatalogCourseList';
import { useCatalogCourses } from '../hooks/useCatalogCourses';

const PAGE_SIZE = 20;

export function StudentCatalogPage() {
  const [page, setPage] = useState(1);
  const coursesQuery = useCatalogCourses(page, PAGE_SIZE);
  const catalog = coursesQuery.data;

  return (
    <section className="mx-auto max-w-5xl space-y-6">
      <header className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <Link
          to="/dashboard"
          className="inline-flex items-center gap-2 text-sm font-semibold text-slate-600 transition hover:text-slate-950"
        >
          <ArrowLeft className="h-4 w-4" />
          В кабинет студента
        </Link>
        <h1 className="mt-4 text-3xl font-semibold text-slate-950">Каталог курсов</h1>
        <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
          Выберите опубликованный курс и запишитесь, чтобы открыть уроки, видео и интерактивные проверки.
        </p>
      </header>

      {coursesQuery.isLoading && (
        <div className="rounded-lg border border-slate-200 bg-white p-6 text-sm text-slate-600 shadow-sm">
          Загружаем курсы...
        </div>
      )}

      {coursesQuery.isError && (
        <div className="rounded-lg border border-rose-200 bg-rose-50 p-6 text-sm text-rose-700">
          Не удалось загрузить каталог курсов.
        </div>
      )}

      {catalog && catalog.items.length === 0 && (
        <div className="rounded-lg border border-dashed border-slate-300 bg-slate-50 p-8 text-sm text-slate-600">
          Опубликованных курсов пока нет.
        </div>
      )}

      {catalog && catalog.items.length > 0 && (
        <>
          <CatalogCourseList courses={catalog.items} />

          <nav className="flex flex-col gap-3 rounded-lg border border-slate-200 bg-white p-4 shadow-sm sm:flex-row sm:items-center sm:justify-between">
            <span className="text-sm text-slate-600">
              Страница {catalog.page} из {catalog.totalPages || 1} · всего {catalog.totalCount}
            </span>
            <div className="flex gap-2">
              <button
                type="button"
                disabled={catalog.page <= 1}
                onClick={() => setPage((current) => Math.max(1, current - 1))}
                className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-700 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <ChevronLeft className="h-4 w-4" />
                Назад
              </button>
              <button
                type="button"
                disabled={catalog.totalPages === 0 || catalog.page >= catalog.totalPages}
                onClick={() => setPage((current) => current + 1)}
                className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-700 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Вперед
                <ChevronRight className="h-4 w-4" />
              </button>
            </div>
          </nav>
        </>
      )}
    </section>
  );
}
