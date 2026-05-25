import { forwardRef, useEffect, useImperativeHandle, useMemo, useRef, useState } from 'react';
import Hls from 'hls.js';
import { apiBaseUrl, getStoredTokens } from '../../../../app/api/client';

type HlsVideoPlayerProps = {
  videoId: string;
  title?: string;
  onTimeUpdate?: (currentTime: number) => void;
  onPlay?: () => void;
  onPause?: () => void;
  onPlaying?: () => void;
  onSeeking?: () => void;
  onSeeked?: () => void;
};

export type HlsVideoPlayerHandle = {
  getCurrentTime: () => number;
  seekTo: (seconds: number, options?: { autoplay?: boolean }) => void;
  play: () => void;
  pause: () => void;
};

function buildApiUrl(path: string) {
  const baseUrl = apiBaseUrl.replace(/\/$/, '');
  return baseUrl ? `${baseUrl}${path}` : path;
}

export const HlsVideoPlayer = forwardRef<HlsVideoPlayerHandle, HlsVideoPlayerProps>(function HlsVideoPlayer(
  { videoId, title, onTimeUpdate, onPlay, onPause, onPlaying, onSeeking, onSeeked },
  ref,
) {
  const videoRef = useRef<HTMLVideoElement | null>(null);
  const onTimeUpdateRef = useRef(onTimeUpdate);
  const onPlayRef = useRef(onPlay);
  const onPauseRef = useRef(onPause);
  const onPlayingRef = useRef(onPlaying);
  const onSeekingRef = useRef(onSeeking);
  const onSeekedRef = useRef(onSeeked);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  onTimeUpdateRef.current = onTimeUpdate;
  onPlayRef.current = onPlay;
  onPauseRef.current = onPause;
  onPlayingRef.current = onPlaying;
  onSeekingRef.current = onSeeking;
  onSeekedRef.current = onSeeked;

  const sourceUrl = useMemo(
    () => buildApiUrl(`/api/v1/media/videos/${videoId}/hls/master.m3u8`),
    [videoId],
  );

  useImperativeHandle(ref, () => ({
    getCurrentTime: () => videoRef.current?.currentTime ?? 0,
    seekTo: (seconds: number, options?: { autoplay?: boolean }) => {
      const video = videoRef.current;

      if (!video) {
        return;
      }

      video.currentTime = Math.max(0, seconds);

      if (options?.autoplay ?? true) {
        void video.play();
      }
    },
    play: () => {
      void videoRef.current?.play();
    },
    pause: () => {
      videoRef.current?.pause();
    },
  }));

  useEffect(() => {
    const video = videoRef.current;

    if (!video) {
      return;
    }

    let hls: Hls | null = null;

    const handleLoadedMetadata = () => {
      setIsLoading(false);
    };

    const handleVideoError = () => {
      setError('Не удалось воспроизвести видео через HLS proxy.');
      setIsLoading(false);
    };

    const handleTimeUpdate = () => {
      onTimeUpdateRef.current?.(video.currentTime);
    };
    const handlePlay = () => onPlayRef.current?.();
    const handlePause = () => onPauseRef.current?.();
    const handlePlaying = () => onPlayingRef.current?.();
    const handleSeeking = () => onSeekingRef.current?.();
    const handleSeeked = () => {
      onSeekedRef.current?.();
      onTimeUpdateRef.current?.(video.currentTime);
    };

    setError(null);
    setIsLoading(true);
    video.addEventListener('loadedmetadata', handleLoadedMetadata);
    video.addEventListener('error', handleVideoError);
    video.addEventListener('timeupdate', handleTimeUpdate);
    video.addEventListener('play', handlePlay);
    video.addEventListener('pause', handlePause);
    video.addEventListener('playing', handlePlaying);
    video.addEventListener('seeking', handleSeeking);
    video.addEventListener('seeked', handleSeeked);

    if (Hls.isSupported()) {
      hls = new Hls({
        xhrSetup: (xhr) => {
          const accessToken = getStoredTokens()?.accessToken;

          if (accessToken) {
            xhr.setRequestHeader('Authorization', `Bearer ${accessToken}`);
          }
        },
      });

      hls.on(Hls.Events.ERROR, (event, data) => {
        void event;

        if (data.fatal) {
          setError(`Ошибка HLS: ${data.details}`);
          setIsLoading(false);
        }
      });

      hls.loadSource(sourceUrl);
      hls.attachMedia(video);
    } else if (video.canPlayType('application/vnd.apple.mpegurl')) {
      video.src = sourceUrl;
    } else {
      setError('Браузер не поддерживает HLS playback.');
      setIsLoading(false);
    }

    return () => {
      video.removeEventListener('loadedmetadata', handleLoadedMetadata);
      video.removeEventListener('error', handleVideoError);
      video.removeEventListener('timeupdate', handleTimeUpdate);
      video.removeEventListener('play', handlePlay);
      video.removeEventListener('pause', handlePause);
      video.removeEventListener('playing', handlePlaying);
      video.removeEventListener('seeking', handleSeeking);
      video.removeEventListener('seeked', handleSeeked);
      hls?.destroy();
      video.removeAttribute('src');
      video.load();
    };
  }, [sourceUrl]);

  return (
    <div className="mt-5 rounded-lg border border-slate-200 bg-slate-950 p-3 shadow-sm">
      <video
        ref={videoRef}
        controls
        className="aspect-video w-full rounded-md bg-black"
        aria-label={title ? `Видео урока: ${title}` : 'Видео урока'}
      />

      <div className="mt-3 flex flex-wrap items-center justify-between gap-2 text-xs text-slate-300">
        <span>{isLoading ? 'Подготовка плеера...' : title ?? 'Видео готово к просмотру'}</span>
        <span className="font-mono">{videoId}</span>
      </div>

      {error && (
        <div className="mt-3 rounded-md border border-rose-400/40 bg-rose-950/40 px-3 py-2 text-sm text-rose-100">
          {error}
        </div>
      )}
    </div>
  );
});
