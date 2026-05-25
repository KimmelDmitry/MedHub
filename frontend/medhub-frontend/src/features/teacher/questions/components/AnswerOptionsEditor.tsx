import { Plus, Trash2 } from 'lucide-react';
import type { AnswerOptionDraft } from '../api/teacherQuestionsApi';

type AnswerOptionsEditorProps = {
  options: AnswerOptionDraft[];
  onChange: (options: AnswerOptionDraft[]) => void;
};

export function AnswerOptionsEditor({ options, onChange }: AnswerOptionsEditorProps) {
  const updateOption = (index: number, patch: Partial<AnswerOptionDraft>) => {
    onChange(options.map((option, optionIndex) => (optionIndex === index ? { ...option, ...patch } : option)));
  };

  const setCorrect = (index: number) => {
    onChange(options.map((option, optionIndex) => ({ ...option, isCorrect: optionIndex === index })));
  };

  const addOption = () => {
    onChange([...options, { text: '', isCorrect: false }]);
  };

  const removeOption = (index: number) => {
    if (options.length <= 2) {
      return;
    }

    const nextOptions = options.filter((_option, optionIndex) => optionIndex !== index);
    const hasCorrect = nextOptions.some((option) => option.isCorrect);
    onChange(hasCorrect ? nextOptions : nextOptions.map((option, optionIndex) => ({ ...option, isCorrect: optionIndex === 0 })));
  };

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between gap-3">
        <label className="text-sm font-semibold text-slate-900">Варианты ответа</label>
        <button
          type="button"
          onClick={addOption}
          className="inline-flex items-center justify-center gap-2 rounded-md border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-900 transition hover:bg-slate-50"
        >
          <Plus className="h-4 w-4" />
          Добавить
        </button>
      </div>

      <div className="space-y-2">
        {options.map((option, index) => (
          <div key={index} className="grid gap-2 rounded-lg border border-slate-200 bg-slate-50 p-3 md:grid-cols-[auto_1fr_auto] md:items-center">
            <input
              type="radio"
              name="correctAnswer"
              checked={option.isCorrect}
              onChange={() => setCorrect(index)}
              className="h-4 w-4 accent-slate-900"
              aria-label="Верный вариант"
            />
            <input
              type="text"
              value={option.text}
              onChange={(event) => updateOption(index, { text: event.target.value })}
              placeholder={`Вариант ${index + 1}`}
              className="min-h-10 rounded-md border border-slate-200 px-3 text-sm outline-none transition focus:border-slate-500"
            />
            <button
              type="button"
              onClick={() => removeOption(index)}
              disabled={options.length <= 2}
              className="inline-flex h-10 items-center justify-center rounded-md border border-rose-200 bg-white px-3 text-rose-700 transition hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-40"
              aria-label="Удалить вариант"
            >
              <Trash2 className="h-4 w-4" />
            </button>
          </div>
        ))}
      </div>

      <p className="text-xs text-slate-500">Для SingleChoice нужен минимум 2 варианта и ровно 1 правильный ответ.</p>
    </div>
  );
}
