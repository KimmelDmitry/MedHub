import { useEffect, useMemo, useRef, useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Link, Navigate, useParams } from 'react-router-dom';
import { AlertTriangle, CheckCircle2 } from 'lucide-react';
import { HlsVideoPlayer, type HlsVideoPlayerHandle } from '../../../teacher/media/components/HlsVideoPlayer';
import {
  attemptResultQueryKey,
  completeAttempt,
  startAttempt,
  studentLessonRuntimeQueryKey,
  submitCheckpointAnswer,
  type CompleteAttemptResponse,
  type StudentRuntimeActiveAttempt,
  type StudentRuntimeCheckpoint,
} from '../api/studentRuntimeApi';
import { StudentAttemptResultPanel } from '../components/StudentAttemptResultPanel';
import { StudentCheckpointModal } from '../components/StudentCheckpointModal';
import { StudentCheckpointTimeline } from '../components/StudentCheckpointTimeline';
import { StudentRuntimeHeader } from '../components/StudentRuntimeHeader';
import { StudentRuntimeLayout } from '../components/StudentRuntimeLayout';
import { useAttemptResult } from '../hooks/useAttemptResult';
import { useStudentCheckpointRuntime } from '../hooks/useStudentCheckpointRuntime';
import { useStudentLessonRuntime } from '../hooks/useStudentLessonRuntime';

function getErrorData(error: unknown): Record<string, unknown> | null {
  if (!error || typeof error !== 'object') {
    return null;
  }

  const response = 'response' in error ? (error.response as unknown) : null;
  const data =
    response && typeof response === 'object' && 'data' in response
      ? (response.data as unknown)
      : null;

  return data && typeof data === 'object' ? (data as Record<string, unknown>) : null;
}

function getErrorMessage(error: unknown) {
  const data = getErrorData(error);

  if (data) {
    return String(
      data.detail ??
        data.message ??
        data.name ??
        data.title ??
        data.code ??
        'Не удалось выполнить запрос.',
    );
  }

  if (error && typeof error === 'object' && 'message' in error) {
    return String((error as { message?: unknown }).message);
  }

  return 'Не удалось выполнить запрос.';
}

function getErrorCode(error: unknown) {
  const data = getErrorData(error);
  return data?.code ? String(data.code) : null;
}

function getSupportedQuestionIds(checkpoints: StudentRuntimeCheckpoint[]) {
  return checkpoints
    .filter((checkpoint) => !checkpoint.status || checkpoint.status.toLowerCase() === 'published')
    .flatMap((checkpoint) =>
      checkpoint.questions
        .filter((question) => {
          const type = String(question.type).trim().toLowerCase();
          return type === 'singlechoice' || type === '1';
        })
        .map((question) => question.questionId),
    );
}

function LoadingState() {
  return (
    <div className="flex min-h-[45vh] items-center justify-center">
      <div className="rounded-lg border border-slate-200 bg-white px-6 py-4 text-sm text-slate-600 shadow-sm">
        Загрузка урока...
      </div>
    </div>
  );
}

export function StudentLessonRuntimePage() {
  const { lessonId } = useParams<{ lessonId: string }>();
  const playerRef = useRef<HlsVideoPlayerHandle | null>(null);
  const startRequestedForLessonRef = useRef<string | null>(null);
  const queryClient = useQueryClient();
  const runtimeQuery = useStudentLessonRuntime(lessonId);
  const [startedAttempt, setStartedAttempt] = useState<StudentRuntimeActiveAttempt | null>(null);
  const [locallyAnsweredQuestionIds, setLocallyAnsweredQuestionIds] = useState<Set<string>>(
    () => new Set(),
  );
  const [completeResult, setCompleteResult] = useState<CompleteAttemptResponse | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const runtime = runtimeQuery.data;
  const attempt = runtime?.activeAttempt ?? startedAttempt;
  const attemptStatus = String(attempt?.status ?? '').toLowerCase();
  const completeStatus = String(completeResult?.status ?? '').toLowerCase();
  const isAttemptCompleted = attemptStatus === 'completed' || completeStatus === 'completed';
  const backendAnsweredQuestionIds = useMemo(
    () => new Set(runtime?.activeAttempt?.answeredQuestionIds ?? []),
    [runtime?.activeAttempt?.answeredQuestionIds],
  );
  const answeredQuestionIds = useMemo(
    () => new Set([...backendAnsweredQuestionIds, ...locallyAnsweredQuestionIds]),
    [backendAnsweredQuestionIds, locallyAnsweredQuestionIds],
  );
  const supportedQuestionIds = useMemo(
    () => getSupportedQuestionIds(runtime?.checkpoints ?? []),
    [runtime?.checkpoints],
  );
  const answeredSupportedCount = supportedQuestionIds.filter((questionId) =>
    answeredQuestionIds.has(questionId),
  ).length;

  const startAttemptMutation = useMutation({
    mutationFn: startAttempt,
    onSuccess: (response) => {
      setStartedAttempt({
        attemptId: response.attemptId,
        status: response.status,
        answeredQuestionIds: [],
      });
      setLocallyAnsweredQuestionIds(new Set());
    },
  });

  const submitAnswerMutation = useMutation({
    mutationFn: ({
      attemptId,
      questionId,
      selectedOptionId,
    }: {
      attemptId: string;
      questionId: string;
      selectedOptionId: string;
    }) =>
      submitCheckpointAnswer(attemptId, {
        questionId,
        selectedOptionIds: [selectedOptionId],
        textAnswer: null,
      }),
  });

  const completeAttemptMutation = useMutation({
    mutationFn: completeAttempt,
    onSuccess: (response) => {
      setCompleteResult(response);
      queryClient.invalidateQueries({ queryKey: studentLessonRuntimeQueryKey(lessonId) });
      queryClient.invalidateQueries({ queryKey: attemptResultQueryKey(response.attemptId) });
    },
  });

  const attemptResultQuery = useAttemptResult(
    attempt?.attemptId,
    Boolean(attempt?.attemptId) && isAttemptCompleted,
  );

  useEffect(() => {
    if (!runtime || completeResult || runtime.activeAttempt) {
      return;
    }

    if (
      lessonId &&
      startRequestedForLessonRef.current !== lessonId &&
      !startAttemptMutation.isPending
    ) {
      startRequestedForLessonRef.current = lessonId;
      startAttemptMutation.mutate(lessonId);
    }
  }, [completeResult, lessonId, runtime, startAttemptMutation]);

  const checkpointRuntime = useStudentCheckpointRuntime({
    checkpoints: runtime?.checkpoints ?? [],
    answeredQuestionIds,
    playerRef,
    enabled: Boolean(attempt?.attemptId) && !isAttemptCompleted,
  });

  if (!lessonId) {
    return <Navigate to="/dashboard" replace />;
  }

  if (runtimeQuery.isLoading) {
    return <LoadingState />;
  }

  if (runtimeQuery.isError || !runtime) {
    const isEnrollmentRequired = getErrorCode(runtimeQuery.error) === 'Enrollment.Required';

    return (
      <StudentRuntimeLayout>
        <section className="rounded-lg border border-rose-200 bg-white p-8 shadow-sm">
          <div className="flex items-start gap-3">
            <AlertTriangle className="mt-1 h-5 w-5 text-rose-600" />
            <div>
              <h1 className="text-2xl font-semibold text-slate-950">
                {isEnrollmentRequired ? 'Вы не записаны на этот курс' : 'Урок недоступен'}
              </h1>
              <p className="mt-2 text-sm leading-6 text-slate-600">
                {isEnrollmentRequired
                  ? 'Запишитесь на курс в каталоге, чтобы открыть урок, видео и интерактивные проверки.'
                  : 'Урок может быть не опубликован, видео еще не готово или у пользователя нет доступа.'}
              </p>
              <Link
                to={isEnrollmentRequired ? '/catalog' : '/dashboard'}
                className="mt-5 inline-flex rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700"
              >
                {isEnrollmentRequired ? 'Перейти в каталог' : 'Вернуться в панель'}
              </Link>
            </div>
          </div>
        </section>
      </StudentRuntimeLayout>
    );
  }

  const attemptId = attempt?.attemptId;
  const startError = startAttemptMutation.error ? getErrorMessage(startAttemptMutation.error) : null;
  const completeError = completeAttemptMutation.error ? getErrorMessage(completeAttemptMutation.error) : null;
  const resultError = attemptResultQuery.error ? getErrorMessage(attemptResultQuery.error) : null;

  const handleAnswered = (questionId: string) => {
    setLocallyAnsweredQuestionIds((current) => {
      const next = new Set(current);
      next.add(questionId);
      return next;
    });
    setStartedAttempt((current) =>
      current
        ? {
            ...current,
            answeredQuestionIds: Array.from(new Set([...current.answeredQuestionIds, questionId])),
          }
        : current,
    );
  };

  const handleSubmitAnswer = async (selectedOptionId: string) => {
    if (!attemptId || !checkpointRuntime.activeRuntime) {
      throw new Error('Попытка еще не создана.');
    }

    setSubmitError(null);

    try {
      return await submitAnswerMutation.mutateAsync({
        attemptId,
        questionId: checkpointRuntime.activeRuntime.question.questionId,
        selectedOptionId,
      });
    } catch (error) {
      const message = getErrorMessage(error);
      setSubmitError(message);
      throw new Error(message, { cause: error });
    }
  };

  const handleCompleteAttempt = () => {
    if (!attemptId) {
      return;
    }

    completeAttemptMutation.mutate(attemptId);
  };

  return (
    <StudentRuntimeLayout>
      <StudentRuntimeHeader
        runtime={runtime}
        answeredCount={answeredSupportedCount}
      />

      {(startError || completeError || resultError) && (
        <div className="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {startError || completeError || resultError}
        </div>
      )}

      {isAttemptCompleted && attemptResultQuery.isLoading && (
        <section className="rounded-lg border border-emerald-200 bg-emerald-50 p-5 text-sm font-medium text-emerald-900">
          Загружаем разбор ответов...
        </section>
      )}

      {attemptResultQuery.data && (
        <StudentAttemptResultPanel result={attemptResultQuery.data} />
      )}

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_320px] lg:items-start">
        <div className="min-w-0 space-y-6">
          <HlsVideoPlayer
            ref={playerRef}
            videoId={runtime.video.videoId}
            title={runtime.video.title ?? runtime.lessonTitle}
            onTimeUpdate={checkpointRuntime.handleTimeUpdate}
            onPlay={checkpointRuntime.handlePlay}
            onPlaying={checkpointRuntime.handlePlaying}
            onSeeking={checkpointRuntime.handleSeeking}
            onSeeked={checkpointRuntime.handleSeeked}
          />

          <StudentCheckpointTimeline
            checkpoints={runtime.checkpoints}
            answeredQuestionIds={answeredQuestionIds}
          />
        </div>

        <aside className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm lg:sticky lg:top-6">
          <h2 className="text-lg font-semibold text-slate-950">Прогресс урока</h2>
          <p className="mt-2 text-sm leading-6 text-slate-600">
            Отвеченные checkpoints сохраняются на backend. После обновления страницы прогресс восстановится.
          </p>

          <div className="mt-5 rounded-lg bg-slate-50 p-4">
            <div className="flex items-center gap-2 text-sm font-medium text-slate-500">
              <CheckCircle2 className="h-4 w-4" />
              Вопросы
            </div>
            <p className="mt-2 text-2xl font-semibold text-slate-950">
              {answeredSupportedCount}
            </p>
            <p className="mt-1 text-xs text-slate-500">пройдено без карты будущих checkpoints</p>
          </div>

          <button
            type="button"
            disabled={!attemptId || completeAttemptMutation.isPending || isAttemptCompleted}
            onClick={handleCompleteAttempt}
            className="mt-5 w-full rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
          >
            {isAttemptCompleted
              ? 'Урок завершен'
              : completeAttemptMutation.isPending
                ? 'Завершение...'
                : 'Завершить урок'}
          </button>
        </aside>
      </div>

      {checkpointRuntime.activeRuntime && (
        <StudentCheckpointModal
          checkpoint={checkpointRuntime.activeRuntime.checkpoint}
          question={checkpointRuntime.activeRuntime.question}
          isSubmitting={submitAnswerMutation.isPending}
          error={submitError}
          onSubmit={handleSubmitAnswer}
          onAnswered={handleAnswered}
          onContinue={checkpointRuntime.continuePlayback}
        />
      )}
    </StudentRuntimeLayout>
  );
}
