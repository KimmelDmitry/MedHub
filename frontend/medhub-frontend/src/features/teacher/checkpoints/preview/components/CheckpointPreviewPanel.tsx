import type { RefObject } from 'react';
import { RotateCcw, ScanLine } from 'lucide-react';
import type { HlsVideoPlayerHandle } from '../../../media/components/HlsVideoPlayer';
import { formatSeconds } from '../../utils/timeFormat';
import type { CheckpointPreviewState } from '../hooks/useCheckpointPreview';
import { CheckpointQuestionModal } from './CheckpointQuestionModal';

type CheckpointPreviewPanelProps = {
  playerRef: RefObject<HlsVideoPlayerHandle | null>;
  isEnabled: boolean;
  onEnabledChange: (enabled: boolean) => void;
  preview: CheckpointPreviewState;
};

export function CheckpointPreviewPanel({
  playerRef,
  isEnabled,
  onEnabledChange,
  preview,
}: CheckpointPreviewPanelProps) {
  return (
    <section className="rounded-lg border border-slate-200 bg-slate-50 p-4">
      <div className="flex flex-col gap-3">
        <div>
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Teacher preview</h2>
          <p className="mt-2 text-sm leading-6 text-slate-700">
            Проверка того, как опубликованные checkpoints остановят видео в student flow.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            onClick={() => {
              preview.resetPreview();
              onEnabledChange(!isEnabled);
            }}
            className={`inline-flex items-center justify-center gap-2 rounded-lg px-3 py-2 text-sm font-semibold transition ${
              isEnabled
                ? 'bg-emerald-700 text-white hover:bg-emerald-600'
                : 'border border-slate-200 bg-white text-slate-900 hover:bg-slate-50'
            }`}
          >
            <ScanLine className="h-4 w-4" />
            {isEnabled ? 'Preview включен' : 'Включить preview'}
          </button>
          <button
            type="button"
            onClick={preview.resetPreview}
            disabled={!isEnabled}
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-900 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <RotateCcw className="h-4 w-4" />
            Reset
          </button>
        </div>
      </div>

      {preview.checkpointsQuery.isLoading && isEnabled ? (
        <div className="mt-4 rounded-lg border border-slate-200 bg-white p-3 text-sm text-slate-600">
          Загрузка опубликованных checkpoints...
        </div>
      ) : preview.checkpointsQuery.isError && isEnabled ? (
        <div className="mt-4 rounded-lg border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
          Не удалось загрузить preview checkpoints.
        </div>
      ) : (
        <div className="mt-4 flex flex-wrap items-center gap-2 text-sm text-slate-600">
          <span className="rounded-md bg-white px-2.5 py-1 font-semibold">
            Published: {preview.checkpoints.length}
          </span>
          <span className="rounded-md bg-white px-2.5 py-1 font-semibold">
            Triggered: {preview.triggeredCount}
          </span>
          {preview.checkpoints.map((checkpoint) => (
            <button
              key={checkpoint.id}
              type="button"
              onClick={() => playerRef.current?.seekTo(checkpoint.timestampSeconds, { autoplay: false })}
              className="rounded-md border border-slate-200 bg-white px-2.5 py-1 text-xs font-semibold text-slate-700 transition hover:bg-slate-50"
            >
              {formatSeconds(checkpoint.timestampSeconds)}
            </button>
          ))}
        </div>
      )}

      {preview.activeCheckpoint && (
        <CheckpointQuestionModal
          key={preview.activeCheckpoint.id}
          checkpoint={preview.activeCheckpoint}
          onContinue={preview.continuePlayback}
        />
      )}
    </section>
  );
}
