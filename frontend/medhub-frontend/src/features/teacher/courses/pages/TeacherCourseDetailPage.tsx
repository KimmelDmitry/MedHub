import { isAxiosError } from 'axios';
import { ArrowLeft, Archive, BookOpen, FilePlus2, Rocket } from 'lucide-react';
import { Link, Navigate, useParams } from 'react-router-dom';
import {
  TeacherLink,
  TeacherPlaceholder,
  TeacherShell,
  teacherSecondaryActionLinkClass,
} from '../../components/TeacherShell';
import { CourseStatusBadge } from '../components/CourseStatusBadge';
import { useArchiveCourse } from '../hooks/useArchiveCourse';
import { usePublishCourse } from '../hooks/usePublishCourse';
import { useTeacherCourse } from '../hooks/useTeacherCourse';
import { useTeacherCourseContent } from '../hooks/useTeacherCourseContent';
import { formatLessonContentType } from '../../lessons/api/teacherLessonsApi';
import { LessonStatusBadge } from '../../lessons/components/LessonStatusBadge';

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

function formatDate(value?: string) {
  if (!value) {
    return 'Неизвестно';
  }

  return new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(new Date(value));
}

export function TeacherCourseDetailPage() {
  const { courseId } = useParams();
  const courseQuery = useTeacherCourse(courseId);
  const contentQuery = useTeacherCourseContent(courseId);
  const publishCourse = usePublishCourse();
  const archiveCourse = useArchiveCourse();

  if (!courseId) {
    return <Navigate to="/teacher/courses" replace />;
  }

  if (courseQuery.isLoading) {
    return (
      <TeacherShell title="Курс" subtitle="Детали курса и структура уроков">
        <div className="rounded-lg border border-slate-200 bg-white p-8 text-sm text-slate-600 shadow-sm">
          Загрузка курса...
        </div>
      </TeacherShell>
    );
  }

  if (courseQuery.isError || !courseQuery.data) {
    return (
      <TeacherShell title="Курс" subtitle="Детали курса и структура уроков">
        <div className="rounded-lg border border-rose-200 bg-white p-8 text-sm text-rose-700 shadow-sm">
          Не удалось загрузить курс: {getErrorMessage(courseQuery.error) ?? 'неизвестная ошибка'}.
        </div>
      </TeacherShell>
    );
  }

  const course = courseQuery.data;
  const lessons = contentQuery.data ?? course.lessons ?? [];
  const status = course.status.trim().toLowerCase();
  const hasLessons = lessons.length > 0;
  const canCreateLesson = status !== 'archived';
  const canPublishCourse = (status === 'draft' || status === 'archived') && hasLessons;
  const publishLabel = status === 'archived' ? 'Опубликовать снова' : 'Опубликовать';
  const actionError = getErrorMessage(publishCourse.error ?? archiveCourse.error);
  const isActionPending = publishCourse.isPending || archiveCourse.isPending;

  return (
    <TeacherShell title={course.title} subtitle="Детали курса и структура уроков">
      <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
          <div className="space-y-4">
            <CourseStatusBadge status={course.status} />
            <p className="max-w-3xl text-sm leading-6 text-slate-600">
              {course.description || 'Описание не заполнено.'}
            </p>
            <p className="text-sm text-slate-500">Создан: {formatDate(course.createdAt)}</p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Link to="/teacher/courses" className={teacherSecondaryActionLinkClass}>
              <ArrowLeft className="h-4 w-4" />
              Назад к курсам
            </Link>
            {canCreateLesson ? (
              <Link to={`/teacher/courses/${course.id}/lessons/new`} className={teacherSecondaryActionLinkClass}>
                <FilePlus2 className="h-4 w-4" />
                Добавить урок
              </Link>
            ) : (
              <button
                type="button"
                disabled
                className="inline-flex cursor-not-allowed items-center justify-center rounded-lg bg-slate-200 px-4 py-3 text-sm font-semibold text-slate-500"
              >
                Уроки нельзя добавить в архив
              </button>
            )}
            {canPublishCourse ? (
              <button
                type="button"
                disabled={isActionPending}
                onClick={() => publishCourse.mutate(course.id)}
                className="inline-flex items-center justify-center gap-2 rounded-lg bg-emerald-700 px-4 py-3 text-sm font-semibold text-white transition hover:bg-emerald-600 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
              >
                <Rocket className="h-4 w-4" />
                {publishLabel}
              </button>
            ) : (
              <button
                type="button"
                disabled
                className="inline-flex cursor-not-allowed items-center justify-center rounded-lg bg-slate-200 px-4 py-3 text-sm font-semibold text-slate-500"
              >
                {status === 'draft' || status === 'archived' ? 'Нужен хотя бы один урок' : 'Опубликовать нельзя'}
              </button>
            )}
            {status === 'published' ? (
              <button
                type="button"
                disabled={isActionPending}
                onClick={() => archiveCourse.mutate(course.id)}
                className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
              >
                <Archive className="h-4 w-4" />
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
        </div>
        {actionError && (
          <div className="mt-5 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {actionError}
          </div>
        )}
        {(status === 'draft' || status === 'archived') && !hasLessons && (
          <div className="mt-5 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
            Добавьте хотя бы один урок, чтобы опубликовать курс.
          </div>
        )}
      </section>

      {contentQuery.isLoading ? (
        <div className="rounded-lg border border-slate-200 bg-white p-8 text-sm text-slate-600 shadow-sm">
          Загрузка уроков...
        </div>
      ) : contentQuery.isError ? (
        <div className="rounded-lg border border-rose-200 bg-white p-8 text-sm text-rose-700 shadow-sm">
          Не удалось загрузить уроки: {getErrorMessage(contentQuery.error) ?? 'неизвестная ошибка'}.
        </div>
      ) : lessons.length === 0 ? (
        <TeacherPlaceholder
          title="Уроков пока нет"
          text="Добавьте первый урок, чтобы собрать структуру курса и подготовить его к публикации."
          action={
            canCreateLesson ? (
              <TeacherLink to={`/teacher/courses/${course.id}/lessons/new`}>
                <FilePlus2 className="h-4 w-4" />
                Добавить урок
              </TeacherLink>
            ) : (
              <button
                type="button"
                disabled
                className="inline-flex cursor-not-allowed items-center justify-center rounded-lg bg-slate-200 px-4 py-3 text-sm font-semibold text-slate-500"
              >
                Курс в архиве
              </button>
            )
          }
        />
      ) : (
        <section className="rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-200 px-5 py-4">
            <h2 className="text-lg font-semibold text-slate-950">Уроки</h2>
          </div>
          <div className="divide-y divide-slate-200">
            {lessons.map((lesson) => (
              <div
                key={lesson.id}
                className="flex flex-col gap-3 px-5 py-4 sm:flex-row sm:items-center sm:justify-between"
              >
                <div>
                  <p className="font-semibold text-slate-950">{lesson.title}</p>
                  <p className="mt-1 text-sm text-slate-500">
                    Порядок: {lesson.orderNumber ?? lesson.order ?? '-'} ·{' '}
                    {formatLessonContentType(lesson.contentType)} ·{' '}
                    {lesson.hasVideo || lesson.videoId ? 'Видео прикреплено' : 'Без видео'}
                  </p>
                </div>
                <div className="flex flex-wrap items-center gap-2">
                  {lesson.status && <LessonStatusBadge status={lesson.status} />}
                  <Link to={`/teacher/lessons/${lesson.id}`} className={teacherSecondaryActionLinkClass}>
                    <BookOpen className="h-4 w-4" />
                    Открыть
                  </Link>
                </div>
              </div>
            ))}
          </div>
        </section>
      )}
    </TeacherShell>
  );
}
