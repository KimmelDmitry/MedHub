import { useMemo, useState, type FormEvent } from 'react';
import { Save } from 'lucide-react';
import type {
  AnswerOptionDraft,
  CreateSingleChoiceQuestionInput,
  TeacherQuestion,
} from '../api/teacherQuestionsApi';
import { AnswerOptionsEditor } from './AnswerOptionsEditor';

export type SingleChoiceQuestionFormSubmitInput = Omit<CreateSingleChoiceQuestionInput, 'answerOptions'> & {
  videoId?: string | null;
  questionId?: string;
  answerOptions: AnswerOptionDraft[];
};

type SingleChoiceQuestionFormProps = {
  checkpointId: string;
  videoId?: string | null;
  question?: TeacherQuestion | null;
  isPending: boolean;
  submitLabel?: string;
  title?: string;
  onSubmit: (input: SingleChoiceQuestionFormSubmitInput) => void;
};

const defaultOptions: AnswerOptionDraft[] = [
  { text: '', isCorrect: true },
  { text: '', isCorrect: false },
];

export function SingleChoiceQuestionForm({
  checkpointId,
  videoId,
  question,
  isPending,
  submitLabel,
  title,
  onSubmit,
}: SingleChoiceQuestionFormProps) {
  const [text, setText] = useState(question?.text ?? '');
  const [allowRetry, setAllowRetry] = useState(question?.allowRetry ?? true);
  const [revealCorrectAnswer, setRevealCorrectAnswer] = useState(question?.revealCorrectAnswer ?? false);
  const [timeLimitSeconds, setTimeLimitSeconds] = useState(
    question?.timeLimitSeconds ? String(question.timeLimitSeconds) : '',
  );
  const [answerOptions, setAnswerOptions] = useState<AnswerOptionDraft[]>(
    question?.answerOptions.map((option) => ({
      id: option.id,
      text: option.text,
      isCorrect: option.isCorrect,
    })) ?? defaultOptions,
  );

  const validationError = useMemo(() => {
    if (!text.trim()) {
      return 'Введите текст вопроса.';
    }

    if (answerOptions.length < 2) {
      return 'Добавьте минимум 2 варианта ответа.';
    }

    if (answerOptions.some((option) => !option.text.trim())) {
      return 'Заполните все варианты ответа.';
    }

    if (answerOptions.filter((option) => option.isCorrect).length !== 1) {
      return 'Выберите ровно 1 правильный ответ.';
    }

    if (timeLimitSeconds.trim() && Number(timeLimitSeconds) <= 0) {
      return 'Лимит времени должен быть больше 0 секунд.';
    }

    return null;
  }, [answerOptions, text, timeLimitSeconds]);

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (validationError || isPending) {
      return;
    }

    onSubmit({
      checkpointId,
      videoId,
      questionId: question?.id,
      text: text.trim(),
      allowRetry,
      revealCorrectAnswer,
      timeLimitSeconds: timeLimitSeconds.trim() ? Number(timeLimitSeconds) : null,
      answerOptions: answerOptions.map((option) => ({
        id: option.id,
        text: option.text.trim(),
        isCorrect: option.isCorrect,
      })),
    });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-5 rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
      <div>
        <h2 className="text-lg font-semibold text-slate-950">{title ?? 'SingleChoice вопрос'}</h2>
        <p className="mt-2 text-sm leading-6 text-slate-600">
          Один вопрос с несколькими вариантами ответа и ровно одним правильным вариантом.
        </p>
      </div>

      <label className="block space-y-2">
        <span className="text-sm font-semibold text-slate-900">Текст вопроса</span>
        <textarea
          value={text}
          onChange={(event) => setText(event.target.value)}
          rows={3}
          className="w-full resize-none rounded-md border border-slate-200 px-3 py-2 text-sm outline-none transition focus:border-slate-500"
          placeholder="Например: Какой орган перекачивает кровь?"
        />
      </label>

      <div className="grid gap-4 md:grid-cols-3">
        <label className="flex items-center gap-3 rounded-lg border border-slate-200 bg-slate-50 px-3 py-3 text-sm font-medium text-slate-700">
          <input
            type="checkbox"
            checked={allowRetry}
            onChange={(event) => setAllowRetry(event.target.checked)}
            className="h-4 w-4 accent-slate-900"
          />
          Разрешить повтор
        </label>
        <label className="flex items-center gap-3 rounded-lg border border-slate-200 bg-slate-50 px-3 py-3 text-sm font-medium text-slate-700">
          <input
            type="checkbox"
            checked={revealCorrectAnswer}
            onChange={(event) => setRevealCorrectAnswer(event.target.checked)}
            className="h-4 w-4 accent-slate-900"
          />
          Показать ответ
        </label>
        <label className="space-y-2">
          <span className="text-sm font-semibold text-slate-900">Лимит, сек.</span>
          <input
            type="number"
            min={1}
            value={timeLimitSeconds}
            onChange={(event) => setTimeLimitSeconds(event.target.value)}
            className="min-h-11 w-full rounded-md border border-slate-200 px-3 text-sm outline-none transition focus:border-slate-500"
            placeholder="Без лимита"
          />
        </label>
      </div>

      <AnswerOptionsEditor options={answerOptions} onChange={setAnswerOptions} />

      {validationError && <p className="text-sm text-amber-700">{validationError}</p>}

      <button
        type="submit"
        disabled={Boolean(validationError) || isPending}
        className="inline-flex items-center justify-center gap-2 rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
      >
        <Save className="h-4 w-4" />
        {isPending ? 'Сохранение...' : submitLabel ?? 'Создать вопрос'}
      </button>
    </form>
  );
}
