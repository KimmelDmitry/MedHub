import { Link } from 'react-router-dom';
import { ArrowLeft, CheckCircle2, Clock3 } from 'lucide-react';
import type { StudentLessonRuntime } from '../api/studentRuntimeApi';

type StudentRuntimeHeaderProps = {
  runtime: StudentLessonRuntime;
  answeredCount: number;
};

export function StudentRuntimeHeader({
  runtime,
  answeredCount,
}: StudentRuntimeHeaderProps) {
  return (
    <header className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div className="min-w-0">
          <Link
            to="/dashboard"
            className="inline-flex items-center gap-2 text-sm font-semibold text-slate-600 transition hover:text-slate-950"
          >
            <ArrowLeft className="h-4 w-4" />
            К панели студента
          </Link>
          <p className="mt-4 text-sm font-medium text-slate-500">{runtime.courseTitle ?? 'Курс'}</p>
          <h1 className="mt-2 text-3xl font-semibold text-slate-950">{runtime.lessonTitle}</h1>
        </div>

        <div className="grid gap-2 sm:grid-cols-2 lg:min-w-[320px]">
          <div className="rounded-lg bg-slate-50 p-4">
            <div className="flex items-center gap-2 text-sm font-medium text-slate-500">
              <CheckCircle2 className="h-4 w-4" />
              Ответы
            </div>
            <p className="mt-2 text-2xl font-semibold text-slate-950">{answeredCount}</p>
            <p className="mt-1 text-xs text-slate-500">пройдено во время просмотра</p>
          </div>
          <div className="rounded-lg bg-slate-50 p-4">
            <div className="flex items-center gap-2 text-sm font-medium text-slate-500">
              <Clock3 className="h-4 w-4" />
              Длительность
            </div>
            <p className="mt-2 text-2xl font-semibold text-slate-950">
              {runtime.video.durationSeconds ? `${Math.ceil(runtime.video.durationSeconds / 60)} мин` : 'Видео'}
            </p>
          </div>
        </div>
      </div>
    </header>
  );
}
