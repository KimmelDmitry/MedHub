import { useMemo, useState, type ReactNode, type RefObject } from 'react';
import { isAxiosError } from 'axios';
import { Link } from 'react-router-dom';
import { Archive, BookOpen, Crosshair, Edit3, ExternalLink, UploadCloud, X } from 'lucide-react';
import type { HlsVideoPlayerHandle } from '../../media/components/HlsVideoPlayer';
import type { CreateCheckpointInput, VideoCheckpoint } from '../api/teacherCheckpointsApi';
import { useArchiveCheckpoint } from '../hooks/useArchiveCheckpoint';
import { useCreateCheckpoint } from '../hooks/useCreateCheckpoint';
import { usePublishCheckpoint } from '../hooks/usePublishCheckpoint';
import { useUpdateCheckpoint } from '../hooks/useUpdateCheckpoint';
import { useVideoCheckpoints } from '../hooks/useVideoCheckpoints';
import { SingleChoiceQuestionForm } from '../../questions/components/SingleChoiceQuestionForm';
import { useCreateQuestion } from '../../questions/hooks/useCreateQuestion';
import { formatSeconds } from '../utils/timeFormat';
import { CheckpointForm } from './CheckpointForm';
import { CheckpointMetadataForm } from './CheckpointMetadataForm';
import { CheckpointStatusBadge } from './CheckpointStatusBadge';

type CheckpointPanelProps = {
  videoId: string;
  playerRef: RefObject<HlsVideoPlayerHandle | null>;
};

type DialogProps = {
  title: string;
  children: ReactNode;
  onClose: () => void;
};

function getCheckpointErrorMessage(error: unknown): string | null {
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
      const detail = record.detail ?? record.message ?? record.title ?? record.name ?? record.Name;

      if (typeof detail === 'string') {
        return detail;
      }

      const code = record.code ?? record.Code;

      if (typeof code === 'string') {
        return code;
      }

      return JSON.stringify(data);
    }

    return error.message;
  }

  return error instanceof Error ? error.message : 'Не удалось выполнить действие.';
}

function normalizeStatus(status: string) {
  return status.trim().toLowerCase();
}

function getCheckpointTitle(checkpoint: VideoCheckpoint) {
  return checkpoint.title?.trim() || 'Checkpoint';
}

function CheckpointDialog({ title, children, onClose }: DialogProps) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/60 px-4 py-6">
      <section className="max-h-[90vh] w-full max-w-3xl overflow-y-auto rounded-lg bg-white p-5 shadow-xl">
        <div className="mb-4 flex items-center justify-between gap-3 border-b border-slate-200 pb-4">
          <h2 className="text-lg font-semibold text-slate-950">{title}</h2>
          <button
            type="button"
            onClick={onClose}
            className="inline-flex h-9 w-9 items-center justify-center rounded-md border border-slate-200 bg-white text-slate-700 transition hover:bg-slate-50"
            aria-label="Закрыть"
          >
            <X className="h-4 w-4" />
          </button>
        </div>
        {children}
      </section>
    </div>
  );
}

export function CheckpointPanel({ videoId, playerRef }: CheckpointPanelProps) {
  const checkpointsQuery = useVideoCheckpoints(videoId);
  const createCheckpoint = useCreateCheckpoint();
  const publishCheckpoint = usePublishCheckpoint();
  const archiveCheckpoint = useArchiveCheckpoint();
  const updateCheckpoint = useUpdateCheckpoint();
  const createQuestion = useCreateQuestion();
  const [formResetKey, setFormResetKey] = useState(0);
  const [editingCheckpoint, setEditingCheckpoint] = useState<VideoCheckpoint | null>(null);
  const [questionCheckpoint, setQuestionCheckpoint] = useState<VideoCheckpoint | null>(null);

  const checkpoints = useMemo(
    () =>
      [...(checkpointsQuery.data ?? [])].sort(
        (left, right) =>
          left.timestampSeconds - right.timestampSeconds || left.orderNumber - right.orderNumber,
      ),
    [checkpointsQuery.data],
  );

  const nextOrderNumber = checkpoints.reduce((max, checkpoint) => Math.max(max, checkpoint.orderNumber), 0) + 1;
  const isActionPending =
    createCheckpoint.isPending ||
    publishCheckpoint.isPending ||
    archiveCheckpoint.isPending ||
    updateCheckpoint.isPending ||
    createQuestion.isPending;
  const actionError = getCheckpointErrorMessage(
    createCheckpoint.error ??
      publishCheckpoint.error ??
      archiveCheckpoint.error ??
      updateCheckpoint.error ??
      createQuestion.error ??
      checkpointsQuery.error,
  );

  const getCurrentVideoTime = () => {
    const currentTime = playerRef.current?.getCurrentTime();
    return typeof currentTime === 'number' && Number.isFinite(currentTime) ? currentTime : null;
  };

  const handleCreate = (input: CreateCheckpointInput) => {
    createCheckpoint.mutate(input, {
      onSuccess: () => setFormResetKey((value) => value + 1),
    });
  };

  const handleSeek = (seconds: number) => {
    playerRef.current?.seekTo(seconds);
  };

  return (
    <section className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
      <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-slate-950">Checkpoints</h2>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
            Создавайте точки остановки, вопросы и публикацию прямо из урока.
          </p>
        </div>
        <span className="rounded-md bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">
          {checkpoints.length}
        </span>
      </div>

      <CheckpointForm
        key={`${formResetKey}-${nextOrderNumber}`}
        videoId={videoId}
        nextOrderNumber={nextOrderNumber}
        isPending={createCheckpoint.isPending}
        onUseCurrentTime={getCurrentVideoTime}
        onSubmit={handleCreate}
      />

      {actionError && (
        <div className="mt-5 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {actionError}
        </div>
      )}

      {checkpoints.length > 0 && (
        <div className="mt-5 flex flex-wrap gap-2">
          {checkpoints.map((checkpoint) => (
            <button
              key={checkpoint.id}
              type="button"
              onClick={() => handleSeek(checkpoint.timestampSeconds)}
              className="inline-flex items-center justify-center rounded-md border border-slate-200 bg-white px-2.5 py-1 text-xs font-semibold text-slate-700 transition hover:border-slate-300 hover:bg-slate-50"
            >
              {formatSeconds(checkpoint.timestampSeconds)}
            </button>
          ))}
        </div>
      )}

      {checkpointsQuery.isLoading ? (
        <div className="mt-5 rounded-lg border border-slate-200 bg-slate-50 p-4 text-sm text-slate-600">
          Загрузка checkpoints...
        </div>
      ) : checkpointsQuery.isError ? (
        <div className="mt-5 rounded-lg border border-rose-200 bg-rose-50 p-4 text-sm text-rose-700">
          Не удалось загрузить checkpoints.
        </div>
      ) : checkpoints.length === 0 ? (
        <div className="mt-5 rounded-lg border border-dashed border-slate-300 bg-slate-50 p-6 text-sm text-slate-600">
          Checkpoints пока нет. Поставьте видео на нужный момент и нажмите “Взять текущий таймкод”.
        </div>
      ) : (
        <div className="mt-5 divide-y divide-slate-200 rounded-lg border border-slate-200">
          {checkpoints.map((checkpoint) => {
            const status = normalizeStatus(checkpoint.status);
            const isDraft = status === 'draft';
            const isPublished = status === 'published';
            const isArchived = status === 'archived';
            const isPublishCandidate = isDraft || isArchived;
            const needsQuestion = checkpoint.isGraded && checkpoint.questionsCount === 0;
            const canPublish = isPublishCandidate && !needsQuestion;
            const canArchive = isPublished;
            const publishLabel = isArchived ? 'Опубликовать снова' : 'Опубликовать';

            return (
              <div key={checkpoint.id} className="grid gap-4 p-4 xl:grid-cols-[140px_1fr_auto] xl:items-center">
                <button
                  type="button"
                  onClick={() => handleSeek(checkpoint.timestampSeconds)}
                  className="inline-flex w-fit items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-semibold text-slate-900 transition hover:bg-slate-50"
                >
                  <Crosshair className="h-4 w-4" />
                  {formatSeconds(checkpoint.timestampSeconds)}
                </button>

                <div>
                  <div className="flex flex-wrap items-center gap-2">
                    <p className="font-semibold text-slate-950">{getCheckpointTitle(checkpoint)}</p>
                    <CheckpointStatusBadge status={checkpoint.status} />
                  </div>
                  <p className="mt-1 text-sm text-slate-500">
                    Порядок: {checkpoint.orderNumber} · {checkpoint.isRequired ? 'Обязательный' : 'Опциональный'} ·{' '}
                    {checkpoint.isGraded ? 'Оцениваемый' : 'Практика'} · Вопросов: {checkpoint.questionsCount}
                  </p>
                  {needsQuestion && isPublishCandidate && (
                    <p className="mt-2 text-sm text-amber-700">
                      Оцениваемый checkpoint нельзя опубликовать без вопроса.
                    </p>
                  )}
                </div>

                <div className="flex flex-wrap gap-2">
                  <button
                    type="button"
                    onClick={() => setEditingCheckpoint(checkpoint)}
                    className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-semibold text-slate-900 transition hover:bg-slate-50"
                  >
                    <Edit3 className="h-4 w-4" />
                    Edit
                  </button>

                  <button
                    type="button"
                    onClick={() => setQuestionCheckpoint(checkpoint)}
                    className={`inline-flex items-center justify-center gap-2 rounded-lg border px-3 py-2 text-sm font-semibold transition ${
                      needsQuestion
                        ? 'border-amber-300 bg-amber-50 text-amber-800 hover:bg-amber-100'
                        : 'border-slate-300 bg-white text-slate-900 hover:bg-slate-50'
                    }`}
                  >
                    <BookOpen className="h-4 w-4" />
                    {needsQuestion ? 'Добавить вопрос' : 'Вопрос'}
                  </button>

                  {canPublish ? (
                    <button
                      type="button"
                      disabled={isActionPending}
                      onClick={() => publishCheckpoint.mutate({ checkpointId: checkpoint.id, videoId })}
                      className="inline-flex items-center justify-center gap-2 rounded-lg bg-emerald-700 px-3 py-2 text-sm font-semibold text-white transition hover:bg-emerald-600 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
                    >
                      <UploadCloud className="h-4 w-4" />
                      {publishLabel}
                    </button>
                  ) : isPublishCandidate ? (
                    <button
                      type="button"
                      disabled
                      className="inline-flex cursor-not-allowed items-center justify-center rounded-lg bg-slate-200 px-3 py-2 text-sm font-semibold text-slate-500"
                    >
                      {needsQuestion ? publishLabel : 'Опубликовать нельзя'}
                    </button>
                  ) : null}

                  {canArchive ? (
                    <button
                      type="button"
                      disabled={isActionPending}
                      onClick={() => archiveCheckpoint.mutate({ checkpointId: checkpoint.id, videoId })}
                      className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-semibold text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
                    >
                      <Archive className="h-4 w-4" />
                      Архивировать
                    </button>
                  ) : null}

                  <Link
                    to={`/teacher/checkpoints/${checkpoint.id}`}
                    className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-700 transition hover:bg-slate-50"
                  >
                    <ExternalLink className="h-4 w-4" />
                    Подробнее
                  </Link>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {editingCheckpoint && (
        <CheckpointDialog title="Редактировать checkpoint" onClose={() => setEditingCheckpoint(null)}>
          <CheckpointMetadataForm
            key={`${editingCheckpoint.id}-${editingCheckpoint.timestampSeconds}-${editingCheckpoint.orderNumber}`}
            checkpoint={{ ...editingCheckpoint, videoId }}
            isSaving={updateCheckpoint.isPending}
            disabledReason={
              normalizeStatus(editingCheckpoint.status) === 'archived'
                ? 'Редактирование archived checkpoint запрещено backend-правилом. Опубликуйте checkpoint снова или создайте новый.'
                : null
            }
            onSave={(input) =>
              updateCheckpoint.mutate(input, {
                onSuccess: () => setEditingCheckpoint(null),
              })
            }
          />
        </CheckpointDialog>
      )}

      {questionCheckpoint && (
        <CheckpointDialog title="Добавить SingleChoice вопрос" onClose={() => setQuestionCheckpoint(null)}>
          <SingleChoiceQuestionForm
            key={questionCheckpoint.id}
            checkpointId={questionCheckpoint.id}
            videoId={videoId}
            isPending={createQuestion.isPending}
            onSubmit={(input) =>
              createQuestion.mutate(input, {
                onSuccess: () => setQuestionCheckpoint(null),
              })
            }
          />
        </CheckpointDialog>
      )}
    </section>
  );
}
