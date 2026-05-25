import { useMemo, useState } from 'react';
import { isAxiosError } from 'axios';
import { Archive, ArrowLeft, BookOpen, Rocket, Trash2 } from 'lucide-react';
import { Link, Navigate, useNavigate, useParams } from 'react-router-dom';
import {
  TeacherShell,
  teacherActionLinkClass,
  teacherSecondaryActionLinkClass,
} from '../../components/TeacherShell';
import { QuestionList } from '../../questions/components/QuestionList';
import { SingleChoiceQuestionForm } from '../../questions/components/SingleChoiceQuestionForm';
import type { TeacherQuestion } from '../../questions/api/teacherQuestionsApi';
import { useCheckpointQuestions } from '../../questions/hooks/useCheckpointQuestions';
import { useCreateQuestion } from '../../questions/hooks/useCreateQuestion';
import { useDeleteQuestion } from '../../questions/hooks/useDeleteQuestion';
import { CheckpointMetadataForm } from '../components/CheckpointMetadataForm';
import { CheckpointStatusBadge } from '../components/CheckpointStatusBadge';
import { useArchiveCheckpoint } from '../hooks/useArchiveCheckpoint';
import { useCheckpointDetail } from '../hooks/useCheckpointDetail';
import { useDeleteCheckpoint } from '../hooks/useDeleteCheckpoint';
import { usePublishCheckpoint } from '../hooks/usePublishCheckpoint';
import { useUpdateCheckpoint } from '../hooks/useUpdateCheckpoint';
import { formatSeconds } from '../utils/timeFormat';

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

function normalizeStatus(status: string) {
  return status.trim().toLowerCase();
}

export function TeacherCheckpointDetailPage() {
  const { checkpointId } = useParams();
  const navigate = useNavigate();
  const checkpointQuery = useCheckpointDetail(checkpointId);
  const questionsQuery = useCheckpointQuestions(checkpointId);
  const createQuestion = useCreateQuestion();
  const deleteQuestion = useDeleteQuestion();
  const publishCheckpoint = usePublishCheckpoint();
  const archiveCheckpoint = useArchiveCheckpoint();
  const updateCheckpoint = useUpdateCheckpoint();
  const deleteCheckpoint = useDeleteCheckpoint();
  const [formResetSignal, setFormResetSignal] = useState(0);

  const questions = useMemo<TeacherQuestion[]>(() => {
    if (questionsQuery.data) {
      return questionsQuery.data;
    }

    const checkpoint = checkpointQuery.data;

    if (!checkpoint) {
      return [];
    }

    return checkpoint.questions.map((question) => ({
      ...question,
      checkpointId: checkpoint.id,
    }));
  }, [checkpointQuery.data, questionsQuery.data]);

  if (!checkpointId) {
    return <Navigate to="/teacher/courses" replace />;
  }

  if (checkpointQuery.isLoading) {
    return (
      <TeacherShell title="Checkpoint" subtitle="Настройка вопросов">
        <div className="rounded-lg border border-slate-200 bg-white p-8 text-sm text-slate-600 shadow-sm">
          Загружаем checkpoint...
        </div>
      </TeacherShell>
    );
  }

  if (checkpointQuery.isError || !checkpointQuery.data) {
    return (
      <TeacherShell title="Checkpoint" subtitle="Настройка вопросов">
        <div className="rounded-lg border border-rose-200 bg-white p-8 text-sm text-rose-700 shadow-sm">
          Не удалось загрузить checkpoint.
        </div>
      </TeacherShell>
    );
  }

  const checkpoint = checkpointQuery.data;
  const status = normalizeStatus(checkpoint.status);
  const isDraft = status === 'draft';
  const isPublished = status === 'published';
  const isArchived = status === 'archived';
  const isPublishCandidate = isDraft || isArchived;
  const questionsCount = questions.length;
  const needsQuestion = checkpoint.isGraded && questionsCount === 0;
  const canPublish = isPublishCandidate && !needsQuestion;
  const canArchive = isPublished;
  const canDeleteCheckpoint = !isPublished;
  const publishLabel = isArchived ? 'Опубликовать снова' : 'Опубликовать';
  const metadataEditDisabledReason = isArchived
    ? 'Редактирование архивного checkpoint сейчас запрещено backend-правилом. Опубликуйте checkpoint снова или создайте новый.'
    : null;
  const actionError = getErrorMessage(
    createQuestion.error ??
      deleteQuestion.error ??
      publishCheckpoint.error ??
      archiveCheckpoint.error ??
      updateCheckpoint.error ??
      deleteCheckpoint.error ??
      questionsQuery.error,
  );

  const handleDeleteQuestion = (questionId: string) => {
    if (!window.confirm('Удалить вопрос?')) {
      return;
    }

    deleteQuestion.mutate({ questionId, checkpointId: checkpoint.id, videoId: checkpoint.videoId });
  };

  const handleDeleteCheckpoint = () => {
    if (!window.confirm('Удалить checkpoint? Это действие нельзя отменить.')) {
      return;
    }

    deleteCheckpoint.mutate(
      { checkpointId: checkpoint.id, videoId: checkpoint.videoId },
      {
        onSuccess: () => navigate(-1),
      },
    );
  };

  return (
    <TeacherShell title={checkpoint.title?.trim() || 'Checkpoint'} subtitle="Настройка вопросов">
      <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
          <div className="space-y-4">
            <CheckpointStatusBadge status={checkpoint.status} />
            <dl className="grid gap-3 text-sm text-slate-600 sm:grid-cols-2">
              <div>
                <dt className="font-medium text-slate-900">Таймкод</dt>
                <dd className="mt-1">{formatSeconds(checkpoint.timestampSeconds)}</dd>
              </div>
              <div>
                <dt className="font-medium text-slate-900">Порядок</dt>
                <dd className="mt-1">{checkpoint.orderNumber}</dd>
              </div>
              <div>
                <dt className="font-medium text-slate-900">Тип</dt>
                <dd className="mt-1">{checkpoint.isGraded ? 'Оцениваемый' : 'Практика'}</dd>
              </div>
              <div>
                <dt className="font-medium text-slate-900">Обязательность</dt>
                <dd className="mt-1">{checkpoint.isRequired ? 'Обязательный' : 'Опциональный'}</dd>
              </div>
              <div>
                <dt className="font-medium text-slate-900">Вопросы</dt>
                <dd className="mt-1">{questionsCount}</dd>
              </div>
            </dl>
            {needsQuestion && (
              <p className="max-w-2xl rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
                Добавьте хотя бы один вопрос, чтобы опубликовать checkpoint.
              </p>
            )}
          </div>

          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              onClick={() => navigate(-1)}
              className={teacherSecondaryActionLinkClass}
            >
              <ArrowLeft className="h-4 w-4" />
              Назад
            </button>
            <Link to="/teacher/courses" className={teacherSecondaryActionLinkClass}>
              К курсам
            </Link>
            {canPublish ? (
              <button
                type="button"
                disabled={publishCheckpoint.isPending}
                onClick={() => publishCheckpoint.mutate({ checkpointId: checkpoint.id, videoId: checkpoint.videoId })}
                className={teacherActionLinkClass}
              >
                <Rocket className="h-4 w-4" />
                {publishCheckpoint.isPending ? 'Публикация...' : publishLabel}
              </button>
            ) : isPublishCandidate ? (
              <button
                type="button"
                disabled
                className="inline-flex cursor-not-allowed items-center justify-center gap-2 rounded-lg bg-slate-200 px-4 py-3 text-sm font-semibold text-slate-500"
              >
                <Rocket className="h-4 w-4" />
                {publishLabel}
              </button>
            ) : null}
            {canArchive && (
              <button
                type="button"
                disabled={archiveCheckpoint.isPending}
                onClick={() => archiveCheckpoint.mutate({ checkpointId: checkpoint.id, videoId: checkpoint.videoId })}
                className={teacherSecondaryActionLinkClass}
              >
                <Archive className="h-4 w-4" />
                {archiveCheckpoint.isPending ? 'Архивация...' : 'Архивировать'}
              </button>
            )}
          </div>
        </div>
      </section>

      {actionError && (
        <div className="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {actionError}
        </div>
      )}

      <CheckpointMetadataForm
        key={`${checkpoint.id}-${checkpoint.timestampSeconds}-${checkpoint.orderNumber}-${checkpoint.status}`}
        checkpoint={checkpoint}
        isSaving={updateCheckpoint.isPending}
        disabledReason={metadataEditDisabledReason}
        onSave={(input) => updateCheckpoint.mutate(input)}
      />

      <section className="space-y-4">
        <div className="flex flex-col gap-2 md:flex-row md:items-end md:justify-between">
          <div>
            <h2 className="text-xl font-semibold text-slate-950">Вопросы checkpoint</h2>
            <p className="mt-2 text-sm leading-6 text-slate-600">
              Сейчас поддерживается SingleChoice: несколько вариантов и один правильный ответ.
            </p>
          </div>
          <span className="inline-flex w-fit items-center gap-2 rounded-md bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">
            <BookOpen className="h-3.5 w-3.5" />
            {questionsCount}
          </span>
        </div>

        {questionsQuery.isLoading ? (
          <div className="rounded-lg border border-slate-200 bg-white p-6 text-sm text-slate-600 shadow-sm">
            Загружаем вопросы...
          </div>
        ) : questionsQuery.isError ? (
          <div className="rounded-lg border border-rose-200 bg-rose-50 p-6 text-sm text-rose-700">
            Не удалось загрузить вопросы.
          </div>
        ) : (
          <QuestionList
            questions={questions}
            isDeleting={deleteQuestion.isPending}
            onDelete={handleDeleteQuestion}
          />
        )}
      </section>

      <SingleChoiceQuestionForm
        key={formResetSignal}
        checkpointId={checkpoint.id}
        videoId={checkpoint.videoId}
        isPending={createQuestion.isPending}
        onSubmit={(input) =>
          createQuestion.mutate(input, {
            onSuccess: () => setFormResetSignal((value) => value + 1),
          })
        }
      />

      <section className="rounded-lg border border-rose-200 bg-white p-6 shadow-sm">
        <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
          <div>
            <h2 className="text-lg font-semibold text-rose-950">Удаление checkpoint</h2>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-rose-700">
              Удаление уберет checkpoint, его вопросы и маркер из видео.
            </p>
          </div>
          <button
            type="button"
            disabled={deleteCheckpoint.isPending || !canDeleteCheckpoint}
            onClick={handleDeleteCheckpoint}
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-rose-300 bg-white px-4 py-3 text-sm font-semibold text-rose-700 transition hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <Trash2 className="h-4 w-4" />
            {deleteCheckpoint.isPending
              ? 'Удаление...'
              : canDeleteCheckpoint
                ? 'Удалить checkpoint'
                : 'Сначала архивируйте'}
          </button>
        </div>
      </section>
    </TeacherShell>
  );
}
