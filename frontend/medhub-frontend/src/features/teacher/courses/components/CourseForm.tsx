import { useState, type FormEvent } from 'react';

export type CourseFormValues = {
  title: string;
  description?: string | null;
};

export function CourseForm({
  isSubmitting,
  error,
  onCancel,
  onSubmit,
}: {
  isSubmitting: boolean;
  error?: string | null;
  onCancel: () => void;
  onSubmit: (values: CourseFormValues) => Promise<void>;
}) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [validationError, setValidationError] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const normalizedTitle = title.trim();
    const normalizedDescription = description.trim();

    if (!normalizedTitle) {
      setValidationError('Введите название курса.');
      return;
    }

    setValidationError(null);

    try {
      await onSubmit({
        title: normalizedTitle,
        description: normalizedDescription.length > 0 ? normalizedDescription : null,
      });
    } catch {
      // The mutation error is rendered by the parent form state.
    }
  };

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <div className="grid gap-4">
        <label className="block">
          <span className="text-sm font-medium text-slate-700">Название курса</span>
          <input
            type="text"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            disabled={isSubmitting}
            placeholder="Например: Анатомия человека"
            className="mt-1 block w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 outline-none transition focus:border-slate-500 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-500"
          />
        </label>

        <label className="block">
          <span className="text-sm font-medium text-slate-700">Описание</span>
          <textarea
            value={description}
            onChange={(event) => setDescription(event.target.value)}
            disabled={isSubmitting}
            rows={5}
            placeholder="Краткое описание курса. Можно заполнить позже."
            className="mt-1 block w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 outline-none transition focus:border-slate-500 disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-500"
          />
          <span className="mt-1 block text-xs text-slate-500">Описание необязательно.</span>
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
          {isSubmitting ? 'Создание...' : 'Создать курс'}
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
