import { useState, type FormEvent } from 'react';
import {
  lessonContentTypeOptions,
  type LessonContentTypeValue,
  type CreateTeacherLessonPayload,
} from '../api/teacherLessonsApi';

export type LessonFormValues = Omit<CreateTeacherLessonPayload, 'courseId'>;

export function LessonForm({
  initialOrder,
  isSubmitting,
  error,
  onCancel,
  onSubmit,
}: {
  initialOrder: number;
  isSubmitting: boolean;
  error?: string | null;
  onCancel: () => void;
  onSubmit: (values: LessonFormValues) => Promise<void>;
}) {
  const [title, setTitle] = useState('');
  const [order, setOrder] = useState(String(initialOrder));
  const [contentType, setContentType] = useState<LessonContentTypeValue>(2);
  const [contentUrl, setContentUrl] = useState('');
  const [validationError, setValidationError] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const normalizedTitle = title.trim();
    const normalizedContent = contentUrl.trim();
    const normalizedOrder = Number(order);

    if (!normalizedTitle) {
      setValidationError('Введите название урока.');
      return;
    }

    if (normalizedTitle.length < 3) {
      setValidationError('Название урока должно быть не короче 3 символов.');
      return;
    }

    if (!Number.isInteger(normalizedOrder) || normalizedOrder <= 0) {
      setValidationError('Порядок урока должен быть целым числом больше 0.');
      return;
    }

    if (contentType === 2 && !normalizedContent) {
      setValidationError('Для текстового урока нужен контент или ссылка.');
      return;
    }

    setValidationError(null);

    try {
      await onSubmit({
        title: normalizedTitle,
        order: normalizedOrder,
        contentType,
        contentUrl: normalizedContent.length > 0 ? normalizedContent : null,
      });
    } catch {
      // The mutation error is rendered by the parent form state.
    }
  };

  const isTextLesson = contentType === 2;

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <div className="grid gap-4">
        <label className="block">
          <span className="text-sm font-medium text-slate-700">Название урока</span>
          <input
            type="text"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            disabled={isSubmitting}
            placeholder="Например: Введение"
            className="mt-1 block w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 outline-none transition focus:border-slate-500 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-500"
          />
        </label>

        <div className="grid gap-4 md:grid-cols-[160px_1fr]">
          <label className="block">
            <span className="text-sm font-medium text-slate-700">Порядок</span>
            <input
              type="number"
              min={1}
              step={1}
              value={order}
              onChange={(event) => setOrder(event.target.value)}
              disabled={isSubmitting}
              className="mt-1 block w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 outline-none transition focus:border-slate-500 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-500"
            />
          </label>

          <label className="block">
            <span className="text-sm font-medium text-slate-700">Тип контента</span>
            <select
              value={contentType}
              onChange={(event) => setContentType(Number(event.target.value) as LessonContentTypeValue)}
              disabled={isSubmitting}
              className="mt-1 block w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 outline-none transition focus:border-slate-500 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-500"
            >
              {lessonContentTypeOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
            <span className="mt-1 block text-xs text-slate-500">
              {lessonContentTypeOptions.find((option) => option.value === contentType)?.description}
            </span>
          </label>
        </div>

        <label className="block">
          <span className="text-sm font-medium text-slate-700">Контент или URL</span>
          <textarea
            value={contentUrl}
            onChange={(event) => setContentUrl(event.target.value)}
            disabled={isSubmitting}
            rows={5}
            placeholder={isTextLesson ? 'Например: краткий текст урока или ссылка' : 'Можно заполнить позже'}
            className="mt-1 block w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 outline-none transition focus:border-slate-500 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-500"
          />
          <span className="mt-1 block text-xs text-slate-500">
            {isTextLesson
              ? 'Для текстового урока поле обязательно. Для публикации любого урока контент тоже должен быть заполнен.'
              : 'Можно оставить пустым для черновика, но публикация потребует контент.'}
          </span>
        </label>
      </div>

      {(validationError || error) && (
        <div className="mt-4 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {validationError ?? error}
        </div>
      )}

      <div className="mt-6 flex flex-wrap gap-3">
        <button
          type="submit"
          disabled={isSubmitting}
          className="inline-flex items-center justify-center rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
        >
          {isSubmitting ? 'Создание...' : 'Создать урок'}
        </button>
        <button
          type="button"
          onClick={onCancel}
          disabled={isSubmitting}
          className="inline-flex items-center justify-center rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:border-slate-300 hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
        >
          Отмена
        </button>
      </div>
    </form>
  );
}
