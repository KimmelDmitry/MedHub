import { useState, type ChangeEvent, type Ref } from 'react';
import { RefreshCw, Upload, X } from 'lucide-react';
import { useVideoPlayback, useVideoStatus } from '../hooks/useVideoStatus';
import { useVideoUpload, type VideoUploadPhase } from '../hooks/useVideoUpload';
import { HlsVideoPlayer, type HlsVideoPlayerHandle } from './HlsVideoPlayer';
import { VideoStatusBadge } from './VideoStatusBadge';

const phaseLabel: Record<VideoUploadPhase, string> = {
  idle: 'Ожидание файла',
  validating: 'Проверка файла',
  starting: 'Подготовка upload',
  uploading: 'Загрузка частей',
  completing: 'Завершение upload',
  processing: 'Обработка видео',
  ready: 'Видео готово',
  failed: 'Ошибка',
};

function isReadyStatus(status?: string | null) {
  return status?.trim().toLowerCase() === 'ready';
}

function formatFileSize(bytes: number) {
  if (bytes < 1024 * 1024) {
    return `${Math.max(1, Math.round(bytes / 1024))} KB`;
  }

  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

export function VideoUploadPanel({
  lessonId,
  courseId,
  videoId,
  playerRef,
  onTimeUpdate,
  onPlay,
  onPause,
  onPlaying,
  onSeeking,
  onSeeked,
}: {
  lessonId: string;
  courseId?: string;
  videoId?: string | null;
  playerRef?: Ref<HlsVideoPlayerHandle>;
  onTimeUpdate?: (currentTime: number) => void;
  onPlay?: () => void;
  onPause?: () => void;
  onPlaying?: () => void;
  onSeeking?: () => void;
  onSeeked?: () => void;
}) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const upload = useVideoUpload(lessonId, courseId);
  const statusVideoId = videoId ?? (upload.phase === 'ready' ? upload.videoId : null);
  const statusQuery = useVideoStatus(statusVideoId);
  const displayStatus = statusQuery.data ?? upload.status;
  const playbackQuery = useVideoPlayback(statusVideoId, isReadyStatus(displayStatus?.status));
  const playback = playbackQuery.data ?? upload.playback;
  const isBusy =
    upload.phase === 'validating' ||
    upload.phase === 'starting' ||
    upload.phase === 'uploading' ||
    upload.phase === 'completing' ||
    upload.phase === 'processing';

  const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    setSelectedFile(event.target.files?.[0] ?? null);
  };

  const handleUpload = async () => {
    try {
      await upload.upload(selectedFile);
      setSelectedFile(null);
    } catch {
      // The hook owns the visible error state.
    }
  };

  return (
    <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
      <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-slate-950">Видео урока</h2>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
            Видео загружается из карточки конкретного урока. После обработки его можно прикрепить к этому уроку.
          </p>
        </div>
        {displayStatus?.status && <VideoStatusBadge status={displayStatus.status} />}
      </div>

      {videoId ? (
        <div className="mt-5 rounded-lg border border-slate-200 bg-slate-50 p-4 text-sm text-slate-700">
          <p>
            Видео прикреплено: <span className="font-mono text-xs">{videoId}</span>
          </p>
          {statusQuery.isLoading && <p className="mt-2 text-slate-500">Загрузка статуса...</p>}
          {statusQuery.isError && <p className="mt-2 text-rose-700">Не удалось загрузить статус видео.</p>}
        </div>
      ) : (
        <div className="mt-5 grid gap-4">
          <label className="block">
            <span className="text-sm font-medium text-slate-700">MP4 файл</span>
            <input
              type="file"
              accept="video/*"
              disabled={isBusy}
              onChange={handleFileChange}
              className="mt-1 block w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 file:mr-4 file:rounded-md file:border-0 file:bg-slate-900 file:px-3 file:py-2 file:text-sm file:font-semibold file:text-white disabled:cursor-not-allowed disabled:bg-slate-50 disabled:text-slate-500"
            />
          </label>

          {selectedFile && (
            <div className="rounded-lg bg-slate-50 p-3 text-sm text-slate-600">
              <span className="font-semibold text-slate-900">{selectedFile.name}</span> ·{' '}
              {formatFileSize(selectedFile.size)}
            </div>
          )}

          <div className="flex flex-wrap gap-3">
            <button
              type="button"
              disabled={isBusy || !selectedFile}
              onClick={handleUpload}
              className="inline-flex items-center justify-center gap-2 rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
            >
              <Upload className="h-4 w-4" />
              {isBusy ? phaseLabel[upload.phase] : 'Загрузить видео'}
            </button>
            {isBusy && (
              <button
                type="button"
                onClick={upload.cancel}
                className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:border-slate-300 hover:bg-slate-50"
              >
                <X className="h-4 w-4" />
                Отмена
              </button>
            )}
            {upload.phase === 'failed' && (
              <button
                type="button"
                onClick={upload.reset}
                className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:border-slate-300 hover:bg-slate-50"
              >
                <RefreshCw className="h-4 w-4" />
                Сбросить
              </button>
            )}
          </div>
        </div>
      )}

      {upload.phase !== 'idle' && (
        <div className="mt-5 rounded-lg border border-slate-200 bg-slate-50 p-4">
          <div className="flex flex-wrap items-center justify-between gap-3 text-sm">
            <span className="font-semibold text-slate-900">{phaseLabel[upload.phase]}</span>
            <span className="text-slate-600">{upload.progress}%</span>
          </div>
          <div className="mt-3 h-2 overflow-hidden rounded-full bg-slate-200">
            <div className="h-full bg-slate-900 transition-all" style={{ width: `${upload.progress}%` }} />
          </div>
          {upload.totalParts > 0 && (
            <p className="mt-2 text-sm text-slate-500">
              Частей загружено: {upload.uploadedParts}/{upload.totalParts}
            </p>
          )}
          {upload.phase === 'processing' && (
            <p className="mt-2 text-sm text-slate-600">Видео обрабатывается. Это может занять несколько минут.</p>
          )}
        </div>
      )}

      {displayStatus && (
        <dl className="mt-5 grid gap-3 rounded-lg border border-slate-200 bg-white p-4 text-sm text-slate-600 sm:grid-cols-3">
          <div>
            <dt className="font-medium text-slate-900">Статус</dt>
            <dd className="mt-1">{displayStatus.status}</dd>
          </div>
          <div>
            <dt className="font-medium text-slate-900">Длительность</dt>
            <dd className="mt-1">{displayStatus.durationSeconds ? `${displayStatus.durationSeconds} сек.` : '-'}</dd>
          </div>
          <div>
            <dt className="font-medium text-slate-900">Размер</dt>
            <dd className="mt-1">
              {displayStatus.width && displayStatus.height ? `${displayStatus.width}x${displayStatus.height}` : '-'}
            </dd>
          </div>
        </dl>
      )}

      {displayStatus?.errorMessage && (
        <div className="mt-5 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {displayStatus.errorMessage}
        </div>
      )}

      {upload.error && (
        <div className="mt-5 rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {upload.error}
        </div>
      )}

      {statusVideoId && isReadyStatus(displayStatus?.status) && (
        <HlsVideoPlayer
          ref={playerRef}
          videoId={statusVideoId}
          title={playback?.title ?? 'Видео урока'}
          onTimeUpdate={onTimeUpdate}
          onPlay={onPlay}
          onPause={onPause}
          onPlaying={onPlaying}
          onSeeking={onSeeking}
          onSeeked={onSeeked}
        />
      )}

      {playbackQuery.isError && isReadyStatus(displayStatus?.status) && (
        <div className="mt-5 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
          Видео готово, но playback URL пока не удалось получить.
        </div>
      )}
    </section>
  );
}
