import { useState } from 'react';
import { CheckCircle2, Play, XCircle } from 'lucide-react';
import type {
  StudentRuntimeCheckpoint,
  StudentRuntimeQuestion,
  SubmitCheckpointAnswerResponse,
} from '../api/studentRuntimeApi';
import { formatSeconds } from '../utils/timeFormat';
import { SingleChoiceRuntimeQuestion } from './SingleChoiceRuntimeQuestion';

type StudentCheckpointModalProps = {
  checkpoint: StudentRuntimeCheckpoint;
  question: StudentRuntimeQuestion;
  isSubmitting: boolean;
  error?: string | null;
  onSubmit: (selectedOptionId: string) => Promise<SubmitCheckpointAnswerResponse>;
  onAnswered: (questionId: string) => void;
  onContinue: () => void;
};

export function StudentCheckpointModal({
  checkpoint,
  question,
  isSubmitting,
  error,
  onSubmit,
  onAnswered,
  onContinue,
}: StudentCheckpointModalProps) {
  const [selectedOptionId, setSelectedOptionId] = useState<string | null>(null);
  const [submitResult, setSubmitResult] = useState<SubmitCheckpointAnswerResponse | null>(null);
  const [localError, setLocalError] = useState<string | null>(null);
  const canSubmit = Boolean(selectedOptionId) && !isSubmitting && !submitResult;

  const handleSubmit = async () => {
    if (!selectedOptionId) {
      return;
    }

    setLocalError(null);

    try {
      const result = await onSubmit(selectedOptionId);
      setSubmitResult(result);
      onAnswered(question.questionId);
    } catch (submitError) {
      setLocalError(submitError instanceof Error ? submitError.message : 'Не удалось отправить ответ.');
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6">
      <section className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-lg bg-white p-6 shadow-xl">
        <div className="flex flex-col gap-3 border-b border-slate-200 pb-5">
          <span className="w-fit rounded-md bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">
            {formatSeconds(checkpoint.timestampSeconds)}
          </span>
          <h2 className="text-xl font-semibold text-slate-950">
            {checkpoint.title?.trim() || 'Checkpoint'}
          </h2>
          <p className="text-sm leading-6 text-slate-600">
            Видео остановлено. Ответьте на вопрос, чтобы продолжить урок.
          </p>
        </div>

        <div className="mt-5">
          <SingleChoiceRuntimeQuestion
            question={question}
            selectedOptionId={selectedOptionId}
            disabled={isSubmitting || Boolean(submitResult)}
            onSelectedOptionIdChange={setSelectedOptionId}
          />
        </div>

        {(localError || error) && (
          <div className="mt-5 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {localError || error}
          </div>
        )}

        {submitResult && (
          <div
            className={`mt-5 flex items-center gap-2 rounded-lg px-4 py-3 text-sm font-medium ${
              submitResult.isCorrect ? 'bg-emerald-50 text-emerald-800' : 'bg-amber-50 text-amber-800'
            }`}
          >
            {submitResult.isCorrect ? <CheckCircle2 className="h-4 w-4" /> : <XCircle className="h-4 w-4" />}
            {submitResult.isCorrect
              ? 'Верно. Можно продолжить просмотр.'
              : 'Ответ сохранён. Продолжайте урок и разберите материал ещё раз.'}
          </div>
        )}

        <div className="mt-6 flex flex-wrap justify-end gap-2">
          {!submitResult && (
            <button
              type="button"
              disabled={!canSubmit}
              onClick={handleSubmit}
              className="inline-flex items-center justify-center rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
            >
              {isSubmitting ? 'Отправка...' : 'Ответить'}
            </button>
          )}
          <button
            type="button"
            disabled={!submitResult}
            onClick={onContinue}
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <Play className="h-4 w-4" />
            Продолжить
          </button>
        </div>
      </section>
    </div>
  );
}
