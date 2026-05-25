import { Edit3, Trash2 } from 'lucide-react';
import type { TeacherQuestion } from '../api/teacherQuestionsApi';

type QuestionListProps = {
  questions: TeacherQuestion[];
  isDeleting: boolean;
  onDelete: (questionId: string) => void;
  onEdit?: (question: TeacherQuestion) => void;
};

function getQuestionTypeLabel(type: string | number) {
  const value = String(type).toLowerCase();

  if (value === '1' || value === 'singlechoice') {
    return 'SingleChoice';
  }

  return String(type);
}

export function QuestionList({ questions, isDeleting, onDelete, onEdit }: QuestionListProps) {
  if (questions.length === 0) {
    return (
      <div className="rounded-lg border border-dashed border-slate-300 bg-slate-50 p-6 text-sm text-slate-600">
        Вопросов пока нет. Добавьте SingleChoice вопрос, чтобы оцениваемый checkpoint можно было опубликовать.
      </div>
    );
  }

  return (
    <div className="divide-y divide-slate-200 rounded-lg border border-slate-200 bg-white">
      {questions.map((question) => (
        <article key={question.id} className="grid gap-4 p-4 lg:grid-cols-[1fr_auto] lg:items-start">
          <div className="space-y-3">
            <div className="flex flex-wrap items-center gap-2">
              <h3 className="font-semibold text-slate-950">{question.text}</h3>
              <span className="rounded-md bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">
                {getQuestionTypeLabel(question.type)}
              </span>
            </div>
            <p className="text-sm text-slate-500">
              {question.allowRetry ? 'Повтор разрешен' : 'Без повтора'} ·{' '}
              {question.revealCorrectAnswer ? 'Показывать правильный ответ' : 'Не показывать правильный ответ'} ·{' '}
              {question.timeLimitSeconds ? `${question.timeLimitSeconds} сек.` : 'Без лимита'}
            </p>
            <ol className="space-y-2">
              {question.answerOptions.map((option) => (
                <li
                  key={option.id}
                  className={`rounded-md border px-3 py-2 text-sm ${
                    option.isCorrect
                      ? 'border-emerald-200 bg-emerald-50 text-emerald-800'
                      : 'border-slate-200 bg-slate-50 text-slate-700'
                  }`}
                >
                  {option.text}
                  {option.isCorrect ? ' · верный' : ''}
                </li>
              ))}
            </ol>
          </div>

          <div className="flex flex-wrap gap-2 lg:justify-end">
            {onEdit && (
              <button
                type="button"
                onClick={() => onEdit(question)}
                className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-900 transition hover:bg-slate-50"
              >
                <Edit3 className="h-4 w-4" />
                Редакт.
              </button>
            )}
            <button
              type="button"
              disabled={isDeleting}
              onClick={() => onDelete(question.id)}
              className="inline-flex items-center justify-center gap-2 rounded-lg border border-rose-200 bg-white px-3 py-2 text-sm font-semibold text-rose-700 transition hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              <Trash2 className="h-4 w-4" />
              Удалить
            </button>
          </div>
        </article>
      ))}
    </div>
  );
}
