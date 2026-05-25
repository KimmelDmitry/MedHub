import { useMemo, useState } from 'react';
import { CheckCircle2, Play, XCircle } from 'lucide-react';
import type { CheckpointDetail, CheckpointQuestion } from '../../api/teacherCheckpointsApi';
import { formatSeconds } from '../../utils/timeFormat';

type CheckpointQuestionModalProps = {
  checkpoint: CheckpointDetail;
  onContinue: () => void;
};

function isSingleChoice(type: string | number) {
  const normalized = String(type).trim().toLowerCase();
  return normalized === 'singlechoice' || normalized === '1';
}

function getFirstQuestion(checkpoint: CheckpointDetail): CheckpointQuestion | null {
  return checkpoint.questions[0] ?? null;
}

export function CheckpointQuestionModal({ checkpoint, onContinue }: CheckpointQuestionModalProps) {
  const question = getFirstQuestion(checkpoint);
  const [selectedOptionId, setSelectedOptionId] = useState<string | null>(null);
  const [isSubmitted, setIsSubmitted] = useState(false);

  const selectedOption = useMemo(
    () => question?.answerOptions.find((option) => option.id === selectedOptionId) ?? null,
    [question?.answerOptions, selectedOptionId],
  );

  const canPreviewQuestion = question && isSingleChoice(question.type);
  const isCorrect = Boolean(selectedOption?.isCorrect);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 px-4 py-6">
      <section className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-lg bg-white p-6 shadow-xl">
        <div className="flex flex-col gap-3 border-b border-slate-200 pb-5">
          <span className="w-fit rounded-md bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">
            {formatSeconds(checkpoint.timestampSeconds)}
          </span>
          <h2 className="text-xl font-semibold text-slate-950">
            {checkpoint.title?.trim() || 'Checkpoint preview'}
          </h2>
          <p className="text-sm leading-6 text-slate-600">
            Видео остановлено в точке checkpoint. Это локальный teacher preview без сохранения попыток.
          </p>
        </div>

        {!question ? (
          <div className="mt-5 rounded-lg border border-slate-200 bg-slate-50 p-4 text-sm text-slate-700">
            У этого checkpoint пока нет вопроса. Для практического checkpoint можно продолжить просмотр.
          </div>
        ) : !canPreviewQuestion ? (
          <div className="mt-5 rounded-lg border border-amber-200 bg-amber-50 p-4 text-sm text-amber-800">
            Preview для этого типа вопроса ещё не реализован. Сейчас поддерживается только SingleChoice.
          </div>
        ) : (
          <div className="mt-5 space-y-4">
            <h3 className="text-lg font-semibold text-slate-950">{question.text}</h3>
            <div className="space-y-2">
              {question.answerOptions.map((option) => {
                const isSelected = option.id === selectedOptionId;
                const showCorrect = isSubmitted && option.isCorrect;
                const showIncorrect = isSubmitted && isSelected && !option.isCorrect;

                return (
                  <button
                    key={option.id}
                    type="button"
                    disabled={isSubmitted}
                    onClick={() => setSelectedOptionId(option.id)}
                    className={`flex w-full items-center justify-between gap-3 rounded-lg border px-4 py-3 text-left text-sm font-medium transition ${
                      showCorrect
                        ? 'border-emerald-300 bg-emerald-50 text-emerald-800'
                        : showIncorrect
                          ? 'border-rose-300 bg-rose-50 text-rose-800'
                          : isSelected
                            ? 'border-slate-900 bg-slate-100 text-slate-950'
                            : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
                    }`}
                  >
                    <span>{option.text}</span>
                    {showCorrect && <CheckCircle2 className="h-4 w-4" />}
                    {showIncorrect && <XCircle className="h-4 w-4" />}
                  </button>
                );
              })}
            </div>

            {isSubmitted && (
              <div
                className={`rounded-lg px-4 py-3 text-sm ${
                  isCorrect ? 'bg-emerald-50 text-emerald-800' : 'bg-rose-50 text-rose-800'
                }`}
              >
                {isCorrect ? 'Верно. В preview можно продолжить видео.' : 'Неверно. Верный ответ подсвечен.'}
              </div>
            )}
          </div>
        )}

        <div className="mt-6 flex flex-wrap justify-end gap-2">
          {canPreviewQuestion && !isSubmitted && (
            <button
              type="button"
              disabled={!selectedOptionId}
              onClick={() => setIsSubmitted(true)}
              className="inline-flex items-center justify-center rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
            >
              Проверить
            </button>
          )}
          <button
            type="button"
            onClick={onContinue}
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:bg-slate-50"
          >
            <Play className="h-4 w-4" />
            Продолжить
          </button>
        </div>
      </section>
    </div>
  );
}
