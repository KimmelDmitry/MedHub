import { useMemo, useState } from 'react';
import { isAxiosError } from 'axios';
import { Archive, ExternalLink, Plus, Rocket, Trash2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import type { CreateCheckpointInput, VideoCheckpoint } from '../api/teacherCheckpointsApi';
import { CheckpointForm } from './CheckpointForm';
import { CheckpointMetadataForm } from './CheckpointMetadataForm';
import { CheckpointStatusBadge } from './CheckpointStatusBadge';
import { formatSeconds } from '../utils/timeFormat';
import { useArchiveCheckpoint } from '../hooks/useArchiveCheckpoint';
import { useCreateCheckpoint } from '../hooks/useCreateCheckpoint';
import { useDeleteCheckpoint } from '../hooks/useDeleteCheckpoint';
import { usePublishCheckpoint } from '../hooks/usePublishCheckpoint';
import { useUpdateCheckpoint } from '../hooks/useUpdateCheckpoint';
import { QuestionList } from '../../questions/components/QuestionList';
import { SingleChoiceQuestionForm } from '../../questions/components/SingleChoiceQuestionForm';
import type { TeacherQuestion } from '../../questions/api/teacherQuestionsApi';
import { useCheckpointQuestions } from '../../questions/hooks/useCheckpointQuestions';
import { useCreateQuestion } from '../../questions/hooks/useCreateQuestion';
import { useDeleteQuestion } from '../../questions/hooks/useDeleteQuestion';
import { useUpdateSingleChoiceQuestion } from '../../questions/hooks/useUpdateSingleChoiceQuestion';
import { CheckpointPreviewPanel } from '../preview/components/CheckpointPreviewPanel';
import type { CheckpointPreviewState } from '../preview/hooks/useCheckpointPreview';
import type { HlsVideoPlayerHandle } from '../../media/components/HlsVideoPlayer';
import type { RefObject } from 'react';

type CheckpointAuthoringPanelProps = {
  videoId: string;
  checkpoints: VideoCheckpoint[];
  selectedCheckpointId?: string | null;
  onSelectedCheckpointIdChange: (checkpointId: string | null) => void;
  onUseCurrentTime: () => number | null;
  playerRef: RefObject<HlsVideoPlayerHandle | null>;
  isPreviewEnabled: boolean;
  onPreviewEnabledChange: (enabled: boolean) => void;
  preview: CheckpointPreviewState;
};

function normalizeStatus(status: string) {
  return status.trim().toLowerCase();
}

function getCheckpointTitle(checkpoint: VideoCheckpoint) {
  return checkpoint.title?.trim() || 'Checkpoint';
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
      const detail = record.detail ?? record.message ?? record.title ?? record.name ?? record.Name;
      const code = record.code ?? record.Code;

      if (typeof detail === 'string') {
        return detail;
      }

      if (typeof code === 'string') {
        return code;
      }

      return JSON.stringify(data);
    }

    return error.message;
  }

  return error instanceof Error ? error.message : 'Не удалось выполнить действие.';
}

export function CheckpointAuthoringPanel({
  videoId,
  checkpoints,
  selectedCheckpointId,
  onSelectedCheckpointIdChange,
  onUseCurrentTime,
  playerRef,
  isPreviewEnabled,
  onPreviewEnabledChange,
  preview,
}: CheckpointAuthoringPanelProps) {
  const createCheckpoint = useCreateCheckpoint();
  const updateCheckpoint = useUpdateCheckpoint();
  const publishCheckpoint = usePublishCheckpoint();
  const archiveCheckpoint = useArchiveCheckpoint();
  const deleteCheckpoint = useDeleteCheckpoint();
  const questionsQuery = useCheckpointQuestions(selectedCheckpointId);
  const createQuestion = useCreateQuestion();
  const updateQuestion = useUpdateSingleChoiceQuestion();
  const deleteQuestion = useDeleteQuestion();
  const [createResetKey, setCreateResetKey] = useState(0);
  const [isQuestionFormOpen, setIsQuestionFormOpen] = useState(false);
  const [editingQuestion, setEditingQuestion] = useState<TeacherQuestion | null>(null);

  const selectedCheckpoint = useMemo(
    () => checkpoints.find((checkpoint) => checkpoint.id === selectedCheckpointId) ?? null,
    [checkpoints, selectedCheckpointId],
  );
  const nextOrderNumber = checkpoints.reduce((max, checkpoint) => Math.max(max, checkpoint.orderNumber), 0) + 1;
  const questions = questionsQuery.data ?? [];
  const questionsCount = questionsQuery.data ? questions.length : selectedCheckpoint?.questionsCount ?? 0;
  const actionError = getErrorMessage(
    createCheckpoint.error ??
      updateCheckpoint.error ??
      publishCheckpoint.error ??
      archiveCheckpoint.error ??
      deleteCheckpoint.error ??
      createQuestion.error ??
      updateQuestion.error ??
      deleteQuestion.error ??
      questionsQuery.error,
  );

  const handleCreate = (input: CreateCheckpointInput) => {
    createCheckpoint.mutate(input, {
      onSuccess: (checkpointId) => {
        setCreateResetKey((value) => value + 1);
        onSelectedCheckpointIdChange(checkpointId);
      },
    });
  };

  const handleDeleteCheckpoint = () => {
    if (!selectedCheckpoint) {
      return;
    }

    if (!window.confirm('Удалить checkpoint? Это действие нельзя отменить.')) {
      return;
    }

    deleteCheckpoint.mutate(
      { checkpointId: selectedCheckpoint.id, videoId },
      {
        onSuccess: () => {
          onSelectedCheckpointIdChange(null);
        },
      },
    );
  };

  const handleDeleteQuestion = (questionId: string) => {
    if (!selectedCheckpoint || !window.confirm('Удалить вопрос?')) {
      return;
    }

    deleteQuestion.mutate({ questionId, checkpointId: selectedCheckpoint.id, videoId });
  };

  const closeQuestionForm = () => {
    setIsQuestionFormOpen(false);
    setEditingQuestion(null);
  };

  if (!selectedCheckpoint) {
    return (
      <aside className="space-y-5 rounded-lg border border-slate-200 bg-white p-5 shadow-sm xl:sticky xl:top-6">
        <CheckpointPreviewPanel
          playerRef={playerRef}
          isEnabled={isPreviewEnabled}
          onEnabledChange={onPreviewEnabledChange}
          preview={preview}
        />
        <div className="flex items-start justify-between gap-3">
          <div>
            <h2 className="text-lg font-semibold text-slate-950">Authoring</h2>
            <p className="mt-2 text-sm leading-6 text-slate-600">
              Выберите checkpoint под видео или создайте новый в текущей позиции.
            </p>
          </div>
          <span className="rounded-md bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">
            {checkpoints.length}
          </span>
        </div>

        <CheckpointForm
          key={`${createResetKey}-${nextOrderNumber}`}
          videoId={videoId}
          nextOrderNumber={nextOrderNumber}
          isPending={createCheckpoint.isPending}
          onUseCurrentTime={onUseCurrentTime}
          onSubmit={handleCreate}
        />

        {actionError && (
          <div className="mt-5 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {actionError}
          </div>
        )}
      </aside>
    );
  }

  const status = normalizeStatus(selectedCheckpoint.status);
  const isDraft = status === 'draft';
  const isPublished = status === 'published';
  const isArchived = status === 'archived';
  const isPublishCandidate = isDraft || isArchived;
  const needsQuestion = selectedCheckpoint.isGraded && questionsCount === 0;
  const canPublish = isPublishCandidate && !needsQuestion;
  const canArchive = isPublished;
  const canDelete = !isPublished;
  const publishLabel = isArchived ? 'Опубликовать снова' : 'Опубликовать';
  const isBusy =
    createCheckpoint.isPending ||
    updateCheckpoint.isPending ||
    publishCheckpoint.isPending ||
    archiveCheckpoint.isPending ||
    deleteCheckpoint.isPending ||
    createQuestion.isPending ||
    updateQuestion.isPending ||
    deleteQuestion.isPending;

  return (
    <aside className="space-y-5 rounded-lg border border-slate-200 bg-white p-5 shadow-sm xl:sticky xl:top-6">
      <CheckpointPreviewPanel
        playerRef={playerRef}
        isEnabled={isPreviewEnabled}
        onEnabledChange={onPreviewEnabledChange}
        preview={preview}
      />

      <div className="flex flex-col gap-3">
        <div className="flex items-start justify-between gap-3">
          <div>
            <h2 className="text-lg font-semibold text-slate-950">{getCheckpointTitle(selectedCheckpoint)}</h2>
            <p className="mt-1 text-sm text-slate-500">
              {formatSeconds(selectedCheckpoint.timestampSeconds)} · #{selectedCheckpoint.orderNumber}
            </p>
          </div>
          <CheckpointStatusBadge status={selectedCheckpoint.status} />
        </div>

        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            onClick={() => onSelectedCheckpointIdChange(null)}
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-900 transition hover:bg-slate-50"
          >
            <Plus className="h-4 w-4" />
            Новый
          </button>
          <Link
            to={`/teacher/checkpoints/${selectedCheckpoint.id}`}
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-700 transition hover:bg-slate-50"
          >
            <ExternalLink className="h-4 w-4" />
            Полная страница
          </Link>
        </div>
      </div>

      {actionError && (
        <div className="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {actionError}
        </div>
      )}

      <CheckpointMetadataForm
        key={`${selectedCheckpoint.id}-${selectedCheckpoint.timestampSeconds}-${selectedCheckpoint.orderNumber}-${selectedCheckpoint.status}`}
        checkpoint={{ ...selectedCheckpoint, videoId }}
        isSaving={updateCheckpoint.isPending}
        disabledReason={
          isArchived
            ? 'Редактирование archived checkpoint сейчас запрещено backend-правилом. Опубликуйте checkpoint снова или создайте новый.'
            : null
        }
        onSave={(input) => updateCheckpoint.mutate(input)}
      />

      <section className="rounded-lg border border-slate-200 bg-slate-50 p-4">
        <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Публикация</h3>
        {needsQuestion && (
          <p className="mt-3 rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-800">
            Добавьте хотя бы один вопрос, чтобы опубликовать graded checkpoint.
          </p>
        )}
        <div className="mt-4 flex flex-wrap gap-2">
          {canPublish ? (
            <button
              type="button"
              disabled={isBusy}
              onClick={() => publishCheckpoint.mutate({ checkpointId: selectedCheckpoint.id, videoId })}
              className="inline-flex items-center justify-center gap-2 rounded-lg bg-emerald-700 px-3 py-2 text-sm font-semibold text-white transition hover:bg-emerald-600 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
            >
              <Rocket className="h-4 w-4" />
              {publishLabel}
            </button>
          ) : isPublishCandidate ? (
            <button
              type="button"
              disabled
              className="inline-flex cursor-not-allowed items-center justify-center gap-2 rounded-lg bg-slate-200 px-3 py-2 text-sm font-semibold text-slate-500"
            >
              <Rocket className="h-4 w-4" />
              {publishLabel}
            </button>
          ) : null}

          {canArchive && (
            <button
              type="button"
              disabled={isBusy}
              onClick={() => archiveCheckpoint.mutate({ checkpointId: selectedCheckpoint.id, videoId })}
              className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-semibold text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              <Archive className="h-4 w-4" />
              Архивировать
            </button>
          )}
        </div>
      </section>

      <section className="space-y-4">
        <div className="flex items-center justify-between gap-3">
          <div>
            <h3 className="font-semibold text-slate-950">Вопросы</h3>
            <p className="mt-1 text-sm text-slate-500">SingleChoice для teacher preview и будущего student runtime.</p>
          </div>
          <span className="rounded-md bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">
            {questionsCount}
          </span>
        </div>

        {questionsQuery.isLoading ? (
          <div className="rounded-lg border border-slate-200 bg-slate-50 p-4 text-sm text-slate-600">
            Загрузка вопросов...
          </div>
        ) : questionsQuery.isError ? (
          <div className="rounded-lg border border-rose-200 bg-rose-50 p-4 text-sm text-rose-700">
            Не удалось загрузить вопросы.
          </div>
        ) : (
          <QuestionList
            questions={questions}
            isDeleting={deleteQuestion.isPending}
            onDelete={handleDeleteQuestion}
            onEdit={(question) => {
              setEditingQuestion(question);
              setIsQuestionFormOpen(true);
            }}
          />
        )}

        {!isQuestionFormOpen ? (
          <button
            type="button"
            onClick={() => {
              setEditingQuestion(null);
              setIsQuestionFormOpen(true);
            }}
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:bg-slate-50"
          >
            <Plus className="h-4 w-4" />
            Добавить SingleChoice
          </button>
        ) : (
          <div className="space-y-3">
            <SingleChoiceQuestionForm
              key={editingQuestion?.id ?? `new-${selectedCheckpoint.id}`}
              checkpointId={selectedCheckpoint.id}
              videoId={videoId}
              question={editingQuestion}
              isPending={createQuestion.isPending || updateQuestion.isPending}
              title={editingQuestion ? 'Редактировать SingleChoice' : 'Новый SingleChoice вопрос'}
              submitLabel={editingQuestion ? 'Сохранить вопрос' : 'Создать вопрос'}
              onSubmit={(input) => {
                if (editingQuestion) {
                  updateQuestion.mutate(
                    {
                      ...input,
                      questionId: editingQuestion.id,
                      checkpointId: selectedCheckpoint.id,
                      videoId,
                      existingAnswerOptions: editingQuestion.answerOptions,
                    },
                    { onSuccess: closeQuestionForm },
                  );
                  return;
                }

                createQuestion.mutate(input, { onSuccess: closeQuestionForm });
              }}
            />
            <button
              type="button"
              disabled={createQuestion.isPending || updateQuestion.isPending}
              onClick={closeQuestionForm}
              className="inline-flex items-center justify-center rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              Отмена
            </button>
          </div>
        )}
      </section>

      <section className="rounded-lg border border-rose-200 bg-white p-4">
        <h3 className="font-semibold text-rose-950">Удаление checkpoint</h3>
        <p className="mt-2 text-sm leading-6 text-rose-700">
          Удаление уберет checkpoint и связанные вопросы из видео.
        </p>
        <button
          type="button"
          disabled={deleteCheckpoint.isPending || !canDelete}
          onClick={handleDeleteCheckpoint}
          className="mt-4 inline-flex items-center justify-center gap-2 rounded-lg border border-rose-300 bg-white px-4 py-3 text-sm font-semibold text-rose-700 transition hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-60"
        >
          <Trash2 className="h-4 w-4" />
          {canDelete ? 'Удалить checkpoint' : 'Сначала архив'}
        </button>
      </section>
    </aside>
  );
}
