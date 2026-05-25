import { useState, type FormEvent } from 'react';
import { Crosshair, Plus } from 'lucide-react';
import type { CreateCheckpointInput } from '../api/teacherCheckpointsApi';
import { formatSeconds } from '../utils/timeFormat';

type CheckpointFormProps = {
  videoId: string;
  nextOrderNumber: number;
  isPending: boolean;
  onUseCurrentTime: () => number | null;
  onSubmit: (input: CreateCheckpointInput) => void;
};

export function CheckpointForm({
  videoId,
  nextOrderNumber,
  isPending,
  onUseCurrentTime,
  onSubmit,
}: CheckpointFormProps) {
  const [timestampSeconds, setTimestampSeconds] = useState(0);
  const [orderNumber, setOrderNumber] = useState(nextOrderNumber);
  const [title, setTitle] = useState('');
  const [isRequired, setIsRequired] = useState(true);
  const [isGraded, setIsGraded] = useState(true);
  const [validationError, setValidationError] = useState<string | null>(null);

  const handleUseCurrentTime = () => {
    const currentTime = onUseCurrentTime();

    if (currentTime === null) {
      setValidationError('Видео еще не готово отдать текущий таймкод.');
      return;
    }

    setTimestampSeconds(Math.max(0, Math.floor(currentTime)));
    setValidationError(null);
  };

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (timestampSeconds < 0) {
      setValidationError('Таймкод не может быть отрицательным.');
      return;
    }

    if (orderNumber <= 0) {
      setValidationError('Порядковый номер должен быть больше нуля.');
      return;
    }

    setValidationError(null);
    onSubmit({
      videoId,
      timestampSeconds,
      orderNumber,
      title: title.trim() || null,
      isRequired,
      isGraded,
    });
  };

  return (
    <form onSubmit={handleSubmit} className="mt-5 grid gap-4 rounded-lg border border-slate-200 bg-slate-50 p-4">
      <div className="grid gap-4 md:grid-cols-[1fr_140px]">
        <label className="block">
          <span className="text-sm font-medium text-slate-700">Название</span>
          <input
            type="text"
            value={title}
            disabled={isPending}
            onChange={(event) => setTitle(event.target.value)}
            placeholder="Checkpoint"
            className="mt-1 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 outline-none transition focus:border-slate-500 disabled:cursor-not-allowed disabled:bg-slate-100"
          />
        </label>

        <label className="block">
          <span className="text-sm font-medium text-slate-700">Порядок</span>
          <input
            type="number"
            min={1}
            value={orderNumber}
            disabled={isPending}
            onChange={(event) => setOrderNumber(Number(event.target.value))}
            className="mt-1 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 outline-none transition focus:border-slate-500 disabled:cursor-not-allowed disabled:bg-slate-100"
          />
        </label>
      </div>

      <div className="grid gap-3 md:grid-cols-[180px_auto]">
        <label className="block">
          <span className="text-sm font-medium text-slate-700">Таймкод, сек.</span>
          <input
            type="number"
            min={0}
            value={timestampSeconds}
            disabled={isPending}
            onChange={(event) => setTimestampSeconds(Number(event.target.value))}
            className="mt-1 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 outline-none transition focus:border-slate-500 disabled:cursor-not-allowed disabled:bg-slate-100"
          />
        </label>

        <div className="flex flex-wrap items-end gap-3">
          <button
            type="button"
            disabled={isPending}
            onClick={handleUseCurrentTime}
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-semibold text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <Crosshair className="h-4 w-4" />
            Взять текущий таймкод
          </button>
          <span className="pb-2 text-sm text-slate-500">{formatSeconds(timestampSeconds)}</span>
        </div>
      </div>

      <div className="flex flex-wrap gap-4 text-sm text-slate-700">
        <label className="inline-flex items-center gap-2">
          <input
            type="checkbox"
            checked={isRequired}
            disabled={isPending}
            onChange={(event) => setIsRequired(event.target.checked)}
            className="h-4 w-4 rounded border-slate-300"
          />
          Обязательный
        </label>
        <label className="inline-flex items-center gap-2">
          <input
            type="checkbox"
            checked={isGraded}
            disabled={isPending}
            onChange={(event) => setIsGraded(event.target.checked)}
            className="h-4 w-4 rounded border-slate-300"
          />
          Оцениваемый
        </label>
      </div>

      {validationError && (
        <div className="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {validationError}
        </div>
      )}

      <div>
        <button
          type="submit"
          disabled={isPending}
          className="inline-flex items-center justify-center gap-2 rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
        >
          <Plus className="h-4 w-4" />
          {isPending ? 'Создание...' : 'Создать checkpoint'}
        </button>
      </div>
    </form>
  );
}
