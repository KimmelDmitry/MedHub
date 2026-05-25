import { useState, type FormEvent } from 'react';
import { Edit3, Save, X } from 'lucide-react';
import type { UpdateCheckpointInput } from '../api/teacherCheckpointsApi';
import { formatSeconds } from '../utils/timeFormat';

export type CheckpointMetadataTarget = {
  id: string;
  videoId: string;
  timestampSeconds: number;
  orderNumber: number;
  title?: string | null;
  isRequired: boolean;
  isGraded: boolean;
  status: string;
};

type CheckpointMetadataFormProps = {
  checkpoint: CheckpointMetadataTarget;
  isSaving: boolean;
  disabledReason?: string | null;
  onSave: (input: UpdateCheckpointInput) => void;
};

export function CheckpointMetadataForm({
  checkpoint,
  isSaving,
  disabledReason,
  onSave,
}: CheckpointMetadataFormProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [title, setTitle] = useState(checkpoint.title ?? '');
  const [timestampSeconds, setTimestampSeconds] = useState(String(checkpoint.timestampSeconds));
  const [orderNumber, setOrderNumber] = useState(String(checkpoint.orderNumber));
  const [isRequired, setIsRequired] = useState(checkpoint.isRequired);
  const [isGraded, setIsGraded] = useState(checkpoint.isGraded);

  const reset = () => {
    setTitle(checkpoint.title ?? '');
    setTimestampSeconds(String(checkpoint.timestampSeconds));
    setOrderNumber(String(checkpoint.orderNumber));
    setIsRequired(checkpoint.isRequired);
    setIsGraded(checkpoint.isGraded);
  };

  const handleCancel = () => {
    reset();
    setIsEditing(false);
  };

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (disabledReason || isSaving) {
      return;
    }

    onSave({
      checkpointId: checkpoint.id,
      videoId: checkpoint.videoId,
      title: title.trim() || null,
      timestampSeconds: Number(timestampSeconds),
      orderNumber: Number(orderNumber),
      isRequired,
      isGraded,
    });
    setIsEditing(false);
  };

  return (
    <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-slate-950">Metadata checkpoint</h2>
          <p className="mt-2 text-sm leading-6 text-slate-600">
            Название, таймкод, порядок и flags checkpoint.
          </p>
        </div>
        {!isEditing && (
          <button
            type="button"
            disabled={Boolean(disabledReason)}
            onClick={() => setIsEditing(true)}
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <Edit3 className="h-4 w-4" />
            Редактировать checkpoint
          </button>
        )}
      </div>

      {disabledReason && (
        <div className="mt-4 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
          {disabledReason}
        </div>
      )}

      {!isEditing ? (
        <dl className="mt-5 grid gap-3 text-sm text-slate-600 sm:grid-cols-2">
          <div>
            <dt className="font-medium text-slate-900">Название</dt>
            <dd className="mt-1">{checkpoint.title?.trim() || 'Checkpoint'}</dd>
          </div>
          <div>
            <dt className="font-medium text-slate-900">Таймкод</dt>
            <dd className="mt-1">{formatSeconds(checkpoint.timestampSeconds)}</dd>
          </div>
          <div>
            <dt className="font-medium text-slate-900">Порядок</dt>
            <dd className="mt-1">{checkpoint.orderNumber}</dd>
          </div>
          <div>
            <dt className="font-medium text-slate-900">Тип</dt>
            <dd className="mt-1">{checkpoint.isGraded ? 'Оцениваемый' : 'Практика'}</dd>
          </div>
          <div>
            <dt className="font-medium text-slate-900">Обязательность</dt>
            <dd className="mt-1">{checkpoint.isRequired ? 'Обязательный' : 'Опциональный'}</dd>
          </div>
        </dl>
      ) : (
        <form onSubmit={handleSubmit} className="mt-5 grid gap-4">
          <label className="block space-y-2">
            <span className="text-sm font-semibold text-slate-900">Название</span>
            <input
              value={title}
              onChange={(event) => setTitle(event.target.value)}
              className="min-h-11 w-full rounded-md border border-slate-200 px-3 text-sm outline-none transition focus:border-slate-500"
              placeholder="Checkpoint"
            />
          </label>

          <div className="grid gap-4 md:grid-cols-2">
            <label className="block space-y-2">
              <span className="text-sm font-semibold text-slate-900">Таймкод, сек.</span>
              <input
                type="number"
                min={0}
                value={timestampSeconds}
                onChange={(event) => setTimestampSeconds(event.target.value)}
                className="min-h-11 w-full rounded-md border border-slate-200 px-3 text-sm outline-none transition focus:border-slate-500"
              />
              <span className="text-xs text-slate-500">{formatSeconds(Number(timestampSeconds) || 0)}</span>
            </label>

            <label className="block space-y-2">
              <span className="text-sm font-semibold text-slate-900">Порядок</span>
              <input
                type="number"
                min={1}
                value={orderNumber}
                onChange={(event) => setOrderNumber(event.target.value)}
                className="min-h-11 w-full rounded-md border border-slate-200 px-3 text-sm outline-none transition focus:border-slate-500"
              />
            </label>
          </div>

          <div className="flex flex-wrap gap-3">
            <label className="flex items-center gap-3 rounded-lg border border-slate-200 bg-slate-50 px-3 py-3 text-sm font-medium text-slate-700">
              <input
                type="checkbox"
                checked={isRequired}
                onChange={(event) => setIsRequired(event.target.checked)}
                className="h-4 w-4 accent-slate-900"
              />
              Обязательный
            </label>
            <label className="flex items-center gap-3 rounded-lg border border-slate-200 bg-slate-50 px-3 py-3 text-sm font-medium text-slate-700">
              <input
                type="checkbox"
                checked={isGraded}
                onChange={(event) => setIsGraded(event.target.checked)}
                className="h-4 w-4 accent-slate-900"
              />
              Оцениваемый
            </label>
          </div>

          <div className="flex flex-wrap gap-2">
            <button
              type="submit"
              disabled={isSaving || !timestampSeconds || !orderNumber}
              className="inline-flex items-center justify-center gap-2 rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
            >
              <Save className="h-4 w-4" />
              {isSaving ? 'Сохранение...' : 'Сохранить'}
            </button>
            <button
              type="button"
              disabled={isSaving}
              onClick={handleCancel}
              className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              <X className="h-4 w-4" />
              Отмена
            </button>
          </div>
        </form>
      )}
    </section>
  );
}
