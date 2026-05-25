import { useRef, useState } from 'react';
import { Archive, ArrowLeft, Rocket } from 'lucide-react';
import { Link, Navigate, useParams } from 'react-router-dom';
import {
  TeacherShell,
  teacherActionLinkClass,
  teacherSecondaryActionLinkClass,
} from '../../components/TeacherShell';
import { formatLessonContentType } from '../api/teacherLessonsApi';
import { LessonStatusBadge } from '../components/LessonStatusBadge';
import { LessonAuthoringWorkspace } from '../components/LessonAuthoringWorkspace';
import { useArchiveLesson } from '../hooks/useArchiveLesson';
import { useLessonDetail } from '../hooks/useLessonDetail';
import { usePublishLesson } from '../hooks/usePublishLesson';
import { getLessonErrorMessage } from '../lib/getLessonErrorMessage';
import { useCheckpointPreview } from '../../checkpoints/preview/hooks/useCheckpointPreview';
import type { HlsVideoPlayerHandle } from '../../media/components/HlsVideoPlayer';
import { useVideoStatus } from '../../media/hooks/useVideoStatus';

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

export function TeacherLessonDetailPage() {
  const { lessonId } = useParams();
  const playerRef = useRef<HlsVideoPlayerHandle | null>(null);
  const [isPreviewEnabled, setIsPreviewEnabled] = useState(false);
  const lessonQuery = useLessonDetail(lessonId);
  const videoStatusQuery = useVideoStatus(lessonQuery.data?.videoId);
  const publishLesson = usePublishLesson();
  const archiveLesson = useArchiveLesson();
  const checkpointPreview = useCheckpointPreview({
    videoId: lessonQuery.data?.videoId,
    playerRef,
    enabled: isPreviewEnabled,
  });

  if (!lessonId) {
    return <Navigate to="/teacher/courses" replace />;
  }

  if (lessonQuery.isLoading) {
    return (
      <TeacherShell title="Урок" subtitle="Редактор интерактивной лекции">
        <div className="rounded-lg border border-slate-200 bg-white p-8 text-sm text-slate-600 shadow-sm">
          Загрузка урока...
        </div>
      </TeacherShell>
    );
  }

  if (lessonQuery.isError || !lessonQuery.data) {
    return (
      <TeacherShell title="Урок" subtitle="Редактор интерактивной лекции">
        <div className="rounded-lg border border-rose-200 bg-white p-8 text-sm text-rose-700 shadow-sm">
          Не удалось загрузить урок: {getLessonErrorMessage(lessonQuery.error) ?? 'неизвестная ошибка'}.
        </div>
      </TeacherShell>
    );
  }

  const lesson = lessonQuery.data;
  const status = lesson.status.trim().toLowerCase();
  const hasContent = Boolean(lesson.contentUrl?.trim());
  const isVideoReady = videoStatusQuery.data?.status?.trim().toLowerCase() === 'ready';
  const canPublishLesson = (status === 'draft' || status === 'archived') && hasContent;
  const canArchiveLesson = status === 'published';
  const publishLabel = status === 'archived' ? 'Опубликовать снова' : 'Опубликовать';
  const isActionPending = publishLesson.isPending || archiveLesson.isPending;
  const actionError = getLessonErrorMessage(publishLesson.error ?? archiveLesson.error);

  return (
    <TeacherShell title={lesson.title} subtitle="Редактор видео, checkpoints и вопросов">
      <section className="rounded-lg border border-slate-200 bg-white px-5 py-4 shadow-sm">
        <div className="flex flex-col gap-4 xl:flex-row xl:items-center xl:justify-between">
          <div className="min-w-0 space-y-3">
            <div className="flex flex-wrap items-center gap-2">
              <LessonStatusBadge status={lesson.status} />
              <span className="rounded-md bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">
                #{lesson.orderNumber}
              </span>
              <span className="rounded-md bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">
                {formatLessonContentType(lesson.contentType)}
              </span>
              <span className="rounded-md bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">
                Создан: {formatDate(lesson.createdAt)}
              </span>
            </div>
            <p className="max-w-3xl truncate text-sm text-slate-600">
              {lesson.videoId ? `Видео: ${lesson.videoId}` : 'Видео пока не прикреплено'}
            </p>
          </div>

          <div className="flex flex-wrap gap-2">
            <Link to={`/teacher/courses/${lesson.courseId}`} className={teacherSecondaryActionLinkClass}>
              <ArrowLeft className="h-4 w-4" />
              К курсу
            </Link>
            {canPublishLesson ? (
              <button
                type="button"
                disabled={isActionPending}
                onClick={() => publishLesson.mutate({ lessonId: lesson.id, courseId: lesson.courseId })}
                className={teacherActionLinkClass}
              >
                <Rocket className="h-4 w-4" />
                {publishLabel}
              </button>
            ) : status === 'draft' || status === 'archived' ? (
              <button
                type="button"
                disabled
                className="inline-flex cursor-not-allowed items-center justify-center rounded-lg bg-slate-200 px-4 py-3 text-sm font-semibold text-slate-500"
              >
                Нужен контент
              </button>
            ) : null}
            {canArchiveLesson ? (
              <button
                type="button"
                disabled={isActionPending}
                onClick={() => archiveLesson.mutate({ lessonId: lesson.id, courseId: lesson.courseId })}
                className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
              >
                <Archive className="h-4 w-4" />
                Архивировать
              </button>
            ) : null}
          </div>
        </div>

        {!hasContent && status === 'draft' && (
          <div className="mt-4 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
            У урока пока нет контента. Его можно хранить как черновик, но публикация будет отклонена backend-правилами.
          </div>
        )}

        {actionError && (
          <div className="mt-4 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {actionError}
          </div>
        )}
      </section>

      <LessonAuthoringWorkspace
        lesson={lesson}
        isVideoReady={isVideoReady}
        playerRef={playerRef}
        isPreviewEnabled={isPreviewEnabled}
        onPreviewEnabledChange={setIsPreviewEnabled}
        preview={checkpointPreview}
      />
    </TeacherShell>
  );
}
