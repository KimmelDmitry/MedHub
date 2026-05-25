import { isAxiosError } from 'axios';
import { BookOpen, FilePlus2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import {
  TeacherLink,
  TeacherPlaceholder,
  TeacherShell,
  teacherSecondaryActionLinkClass,
} from '../../components/TeacherShell';
import { CourseStatusBadge } from '../components/CourseStatusBadge';
import { useArchiveCourse } from '../hooks/useArchiveCourse';
import { usePublishCourse } from '../hooks/usePublishCourse';
import { useTeacherCourses } from '../hooks/useTeacherCourses';
import type { CourseListItem } from '../api/teacherCoursesApi';

function formatDate(value: string) {
  return new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(new Date(value));
}

function getErrorMessage(error: unknown): string | null {
  if (!error) {
    return null;
  }

  if (isAxiosError(error)) {
    const data = error.response?.data;

    if (typeof data === 'string') {
      return data;
    }

    if (data && typeof data === 'object') {
      const record = data as Record<string, unknown>;
      const detail = record.detail ?? record.message ?? record.title;

      if (typeof detail === 'string') {
        return detail;
      }
    }

    return error.message;
  }

  return error instanceof Error ? error.message : 'Не удалось выполнить действие.';
}

function CourseActions({ course }: { course: CourseListItem }) {
  const publishCourse = usePublishCourse();
  const archiveCourse = useArchiveCourse();
  const status = course.status.trim().toLowerCase();
  const canPublish = (status === 'draft' || status === 'archived') && course.lessonsCount > 0;
  const publishLabel = status === 'archived' ? 'Опубликовать снова' : 'Опубликовать';
  const isBusy = publishCourse.isPending || archiveCourse.isPending;
  const actionError = getErrorMessage(publishCourse.error ?? archiveCourse.error);

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap gap-2">
        <Link to={`/teacher/courses/${course.id}`} className={teacherSecondaryActionLinkClass}>
          <BookOpen className="h-4 w-4" />
          Открыть
        </Link>
        {canPublish ? (
          <button
            type="button"
            disabled={isBusy}
            onClick={() => publishCourse.mutate(course.id)}
            className="inline-flex items-center justify-center rounded-lg bg-emerald-700 px-4 py-3 text-sm font-semibold text-white transition hover:bg-emerald-600 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
          >
            {publishLabel}
          </button>
        ) : (
          <button
            type="button"
            disabled
            className="inline-flex cursor-not-allowed items-center justify-center rounded-lg bg-slate-200 px-4 py-3 text-sm font-semibold text-slate-500"
          >
            {status === 'draft' || status === 'archived' ? 'Добавьте хотя бы один урок' : 'Опубликовать нельзя'}
          </button>
        )}
        {status === 'published' ? (
          <button
            type="button"
            disabled={isBusy}
            onClick={() => archiveCourse.mutate(course.id)}
            className="inline-flex items-center justify-center rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
          >
            Архивировать
          </button>
        ) : (
          status === 'draft' && (
            <button
              type="button"
              disabled
              className="inline-flex cursor-not-allowed items-center justify-center rounded-lg bg-slate-200 px-4 py-3 text-sm font-semibold text-slate-500"
            >
              Архивировать после публикации
            </button>
          )
        )}
      </div>
      {actionError && <p className="text-sm text-rose-700">{actionError}</p>}
    </div>
  );
}

export function TeacherCoursesPage() {
  const coursesQuery = useTeacherCourses();
  const courses = coursesQuery.data ?? [];

  if (coursesQuery.isLoading) {
    return (
      <TeacherShell title="Курсы" subtitle="Список курсов преподавателя и управление публикацией">
        <div className="rounded-lg border border-slate-200 bg-white p-8 text-sm text-slate-600 shadow-sm">
          Загрузка курсов...
        </div>
      </TeacherShell>
    );
  }

  if (coursesQuery.isError) {
    return (
      <TeacherShell title="Курсы" subtitle="Список курсов преподавателя и управление публикацией">
        <div className="rounded-lg border border-rose-200 bg-white p-8 text-sm text-rose-700 shadow-sm">
          Не удалось загрузить курсы: {getErrorMessage(coursesQuery.error) ?? 'неизвестная ошибка'}.
        </div>
      </TeacherShell>
    );
  }

  return (
    <TeacherShell title="Курсы" subtitle="Список курсов преподавателя и управление публикацией">
      <div className="flex flex-wrap items-center justify-between gap-3 rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <p className="text-sm text-slate-600">
          Найдено курсов: <span className="font-semibold text-slate-950">{courses.length}</span>
        </p>
        <TeacherLink to="/teacher/courses/new">
          <FilePlus2 className="h-4 w-4" />
          Создать курс
        </TeacherLink>
      </div>

      {courses.length === 0 ? (
        <TeacherPlaceholder
          title="Курсов пока нет"
          text="Создайте первый курс, чтобы начать собирать структуру обучения."
          action={
            <TeacherLink to="/teacher/courses/new">
              <FilePlus2 className="h-4 w-4" />
              Создать курс
            </TeacherLink>
          }
        />
      ) : (
        <div className="grid gap-4">
          {courses.map((course) => (
            <article key={course.id} className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
              <div className="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
                <div className="min-w-0 space-y-3">
                  <div className="flex flex-wrap items-center gap-3">
                    <h2 className="text-xl font-semibold text-slate-950">{course.title}</h2>
                    <CourseStatusBadge status={course.status} />
                  </div>
                  <p className="max-w-3xl text-sm leading-6 text-slate-600">
                    {course.description || 'Описание не заполнено.'}
                  </p>
                  <dl className="flex flex-wrap gap-x-6 gap-y-2 text-sm text-slate-500">
                    <div>
                      <dt className="inline">Уроков: </dt>
                      <dd className="inline font-semibold text-slate-700">{course.lessonsCount}</dd>
                    </div>
                    <div>
                      <dt className="inline">Создан: </dt>
                      <dd className="inline font-semibold text-slate-700">{formatDate(course.createdOnUtc)}</dd>
                    </div>
                  </dl>
                </div>
                <CourseActions course={course} />
              </div>
            </article>
          ))}
        </div>
      )}
    </TeacherShell>
  );
}
