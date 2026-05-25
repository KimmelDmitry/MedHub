import { Link } from 'react-router-dom';
import { ArrowRight, BookOpen, Clock3 } from 'lucide-react';
import type { StudentDashboardCourse } from '../api/studentDashboardApi';

type StudentDashboardCourseCardProps = {
  course: StudentDashboardCourse;
};

function formatDate(value?: string | null) {
  if (!value) {
    return 'активности пока нет';
  }

  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

export function StudentDashboardCourseCard({ course }: StudentDashboardCourseCardProps) {
  const progress = Math.max(0, Math.min(100, course.progressPercent));
  const continueLesson = course.continueLesson;

  return (
    <article className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
      <div className="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-2">
            <h2 className="text-xl font-semibold text-slate-950">{course.courseTitle}</h2>
            <span className="rounded-md bg-emerald-50 px-2 py-1 text-xs font-semibold text-emerald-700">
              записан
            </span>
          </div>
          <p className="mt-2 line-clamp-2 text-sm leading-6 text-slate-600">
            {course.courseDescription || 'Описание курса появится позже.'}
          </p>

          <div className="mt-5">
            <div className="flex items-center justify-between gap-3 text-sm">
              <span className="font-medium text-slate-700">
                {course.completedLessonsCount}/{course.publishedLessonsCount} уроков
              </span>
              <span className="font-semibold text-slate-950">{progress}%</span>
            </div>
            <div className="mt-2 h-2 rounded-full bg-slate-100">
              <div
                className="h-2 rounded-full bg-slate-900 transition-all"
                style={{ width: `${progress}%` }}
              />
            </div>
          </div>

          <div className="mt-4 flex flex-wrap gap-3 text-xs font-medium text-slate-500">
            <span className="inline-flex items-center gap-1.5">
              <Clock3 className="h-3.5 w-3.5" />
              Последняя активность: {formatDate(course.lastActivityAtUtc)}
            </span>
            <span className="inline-flex items-center gap-1.5">
              <BookOpen className="h-3.5 w-3.5" />
              {course.publishedLessonsCount} опубликованных уроков
            </span>
          </div>
        </div>

        <div className="flex shrink-0 flex-col gap-2 sm:flex-row lg:flex-col">
          {continueLesson ? (
            <Link
              to={`/student/lessons/${continueLesson.lessonId}`}
              className="inline-flex items-center justify-center gap-2 rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700"
            >
              Продолжить
              <ArrowRight className="h-4 w-4" />
            </Link>
          ) : (
            <Link
              to={`/catalog/courses/${course.courseId}`}
              className="inline-flex items-center justify-center gap-2 rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700"
            >
              Открыть курс
              <ArrowRight className="h-4 w-4" />
            </Link>
          )}

          <Link
            to={`/catalog/courses/${course.courseId}`}
            className="inline-flex items-center justify-center rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-700 transition hover:bg-slate-50"
          >
            Курс
          </Link>
        </div>
      </div>
    </article>
  );
}
