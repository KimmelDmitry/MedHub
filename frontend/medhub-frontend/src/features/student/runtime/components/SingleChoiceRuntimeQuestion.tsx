import type { StudentRuntimeQuestion } from '../api/studentRuntimeApi';

type SingleChoiceRuntimeQuestionProps = {
  question: StudentRuntimeQuestion;
  selectedOptionId: string | null;
  disabled?: boolean;
  onSelectedOptionIdChange: (optionId: string) => void;
};

export function SingleChoiceRuntimeQuestion({
  question,
  selectedOptionId,
  disabled = false,
  onSelectedOptionIdChange,
}: SingleChoiceRuntimeQuestionProps) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold text-slate-950">{question.text}</h3>
      <div className="space-y-2">
        {question.answerOptions.map((option) => {
          const isSelected = option.id === selectedOptionId;

          return (
            <button
              key={option.id}
              type="button"
              disabled={disabled}
              onClick={() => onSelectedOptionIdChange(option.id)}
              className={`flex w-full items-center gap-3 rounded-lg border px-4 py-3 text-left text-sm font-medium transition ${
                isSelected
                  ? 'border-slate-900 bg-slate-100 text-slate-950'
                  : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
              } disabled:cursor-not-allowed disabled:opacity-70`}
            >
              <span
                className={`flex h-4 w-4 shrink-0 rounded-full border ${
                  isSelected ? 'border-slate-900 bg-slate-900 ring-2 ring-slate-200' : 'border-slate-300'
                }`}
              />
              <span>{option.text}</span>
            </button>
          );
        })}
      </div>
    </div>
  );
}
