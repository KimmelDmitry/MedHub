import { Crosshair, Plus } from 'lucide-react';
import type { VideoCheckpoint } from '../api/teacherCheckpointsApi';
import { formatSeconds } from '../utils/timeFormat';
import { CheckpointStatusBadge } from './CheckpointStatusBadge';

type CheckpointTimelineProps = {
  checkpoints: VideoCheckpoint[];
  selectedCheckpointId?: string | null;
  isLoading: boolean;
  isError: boolean;
  isCreating: boolean;
  onSelect: (checkpoint: VideoCheckpoint) => void;
  onCreateAtCurrentTime: () => void;
};

function getCheckpointTitle(checkpoint: VideoCheckpoint) {
  return checkpoint.title?.trim() || 'Checkpoint';
}

export function CheckpointTimeline({
  checkpoints,
  selectedCheckpointId,
  isLoading,
  isError,
  isCreating,
  onSelect,
  onCreateAtCurrentTime,
}: CheckpointTimelineProps) {
  return (
    <section className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
      <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-slate-950">Checkpoints</h2>
          <p className="mt-2 text-sm leading-6 text-slate-600">
            Навигация по остановкам видео. Выберите checkpoint, чтобы редактировать его справа.
          </p>
        </div>
        <button
          type="button"
          disabled={isCreating}
          onClick={onCreateAtCurrentTime}
          className="inline-flex items-center justify-center gap-2 rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
        >
          <Plus className="h-4 w-4" />
          Создать в текущей точке
        </button>
      </div>

      {isLoading ? (
        <div className="mt-5 rounded-lg border border-slate-200 bg-slate-50 p-4 text-sm text-slate-600">
          Загрузка checkpoints...
        </div>
      ) : isError ? (
        <div className="mt-5 rounded-lg border border-rose-200 bg-rose-50 p-4 text-sm text-rose-700">
          Не удалось загрузить checkpoints.
        </div>
      ) : checkpoints.length === 0 ? (
        <div className="mt-5 rounded-lg border border-dashed border-slate-300 bg-slate-50 p-5 text-sm text-slate-600">
          Checkpoints пока нет. Поставьте видео на нужный момент и создайте первую остановку.
        </div>
      ) : (
        <div className="mt-5 flex gap-3 overflow-x-auto pb-2">
          {checkpoints.map((checkpoint) => {
            const isSelected = checkpoint.id === selectedCheckpointId;

            return (
              <button
                key={checkpoint.id}
                type="button"
                onClick={() => onSelect(checkpoint)}
                className={`min-w-[220px] rounded-lg border p-3 text-left transition ${
                  isSelected
                    ? 'border-slate-900 bg-slate-950 text-white'
                    : 'border-slate-200 bg-white text-slate-700 hover:border-slate-300 hover:bg-slate-50'
                }`}
              >
                <div className="flex items-center justify-between gap-2">
                  <span
                    className={`inline-flex items-center gap-1.5 rounded-md px-2 py-1 text-xs font-semibold ${
                      isSelected ? 'bg-white/15 text-white' : 'bg-slate-100 text-slate-700'
                    }`}
                  >
                    <Crosshair className="h-3.5 w-3.5" />
                    {formatSeconds(checkpoint.timestampSeconds)}
                  </span>
                  <CheckpointStatusBadge status={checkpoint.status} />
                </div>
                <p className="mt-3 line-clamp-2 text-sm font-semibold">{getCheckpointTitle(checkpoint)}</p>
                <p className={`mt-2 text-xs ${isSelected ? 'text-slate-300' : 'text-slate-500'}`}>
                  #{checkpoint.orderNumber} · {checkpoint.isRequired ? 'Required' : 'Optional'} ·{' '}
                  {checkpoint.isGraded ? 'Graded' : 'Practice'} · {checkpoint.questionsCount} questions
                </p>
              </button>
            );
          })}
        </div>
      )}
    </section>
  );
}
