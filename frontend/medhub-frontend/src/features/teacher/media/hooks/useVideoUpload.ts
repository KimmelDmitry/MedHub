import { useCallback, useRef, useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { isAxiosError } from 'axios';
import { teacherCoursesQueryKey } from '../../courses/api/teacherCoursesApi';
import { teacherLessonQueryKey } from '../../lessons/api/teacherLessonsApi';
import {
  attachVideoToLesson,
  completeVideoUpload,
  getVideoPlayback,
  getVideoStatus,
  startVideoUpload,
  teacherVideoQueryKey,
  type VideoPlaybackResponse,
  type VideoStatusResponse,
} from '../api/teacherMediaApi';
import { uploadChunks } from '../utils/uploadChunks';

export type VideoUploadPhase =
  | 'idle'
  | 'validating'
  | 'starting'
  | 'uploading'
  | 'completing'
  | 'processing'
  | 'ready'
  | 'failed';

export type VideoUploadState = {
  phase: VideoUploadPhase;
  progress: number;
  uploadedParts: number;
  totalParts: number;
  videoId?: string;
  error?: string | null;
  status?: VideoStatusResponse | null;
  playback?: VideoPlaybackResponse | null;
};

const initialState: VideoUploadState = {
  phase: 'idle',
  progress: 0,
  uploadedParts: 0,
  totalParts: 0,
  error: null,
  status: null,
  playback: null,
};

function getErrorMessage(error: unknown): string {
  if (isAxiosError(error)) {
    const data = error.response?.data;

    if (typeof data === 'string') {
      return data;
    }

    if (data && typeof data === 'object') {
      const record = data as Record<string, unknown>;
      const detail = record.detail ?? record.message ?? record.title ?? record.name ?? record.Name;

      if (typeof detail === 'string') {
        return detail;
      }

      return JSON.stringify(data);
    }

    return error.message;
  }

  return error instanceof Error ? error.message : 'Не удалось загрузить видео.';
}

function isReadyStatus(status?: string | null) {
  return status?.trim().toLowerCase() === 'ready';
}

function isFailedStatus(status?: string | null) {
  return status?.trim().toLowerCase() === 'failed';
}

function isTerminalStatus(status?: string | null) {
  return isReadyStatus(status) || isFailedStatus(status);
}

function delay(ms: number, signal: AbortSignal) {
  return new Promise<void>((resolve, reject) => {
    const timeoutId = window.setTimeout(resolve, ms);

    signal.addEventListener(
      'abort',
      () => {
        window.clearTimeout(timeoutId);
        reject(new DOMException('Upload aborted', 'AbortError'));
      },
      { once: true },
    );
  });
}

export function useVideoUpload(lessonId: string, courseId?: string) {
  const queryClient = useQueryClient();
  const abortControllerRef = useRef<AbortController | null>(null);
  const [state, setState] = useState<VideoUploadState>(initialState);

  const reset = useCallback(() => {
    abortControllerRef.current?.abort();
    abortControllerRef.current = null;
    setState(initialState);
  }, []);

  const cancel = useCallback(() => {
    abortControllerRef.current?.abort();
    abortControllerRef.current = null;
    setState((current) => ({
      ...current,
      phase: 'failed',
      error: 'Загрузка отменена.',
    }));
  }, []);

  const upload = useCallback(
    async (file: File | null) => {
      abortControllerRef.current?.abort();
      const abortController = new AbortController();
      abortControllerRef.current = abortController;

      try {
        setState({
          ...initialState,
          phase: 'validating',
        });

        if (!file) {
          throw new Error('Выберите видеофайл.');
        }

        if (!file.type.startsWith('video/')) {
          throw new Error('Выберите файл с типом video/*.');
        }

        if (file.size <= 0) {
          throw new Error('Файл пустой.');
        }

        setState((current) => ({
          ...current,
          phase: 'starting',
        }));

        const startResult = await startVideoUpload({
          lessonId,
          fileName: file.name,
          contentType: file.type,
          sizeBytes: file.size,
        });

        setState((current) => ({
          ...current,
          phase: 'uploading',
          videoId: startResult.videoId,
          totalParts: startResult.chunkUploadUrls.length,
        }));

        const partETags = await uploadChunks({
          file,
          chunkSize: startResult.chunkSize,
          chunkUploadUrls: startResult.chunkUploadUrls,
          signal: abortController.signal,
          onProgress: ({ uploadedBytes, totalBytes, uploadedParts, totalParts }) => {
            setState((current) => ({
              ...current,
              phase: 'uploading',
              progress: Math.round((uploadedBytes / totalBytes) * 100),
              uploadedParts,
              totalParts,
            }));
          },
        });

        setState((current) => ({
          ...current,
          phase: 'completing',
          progress: 100,
        }));

        await completeVideoUpload(startResult.videoId, {
          uploadId: startResult.uploadId,
          partETags,
        });

        setState((current) => ({
          ...current,
          phase: 'processing',
        }));

        let latestStatus: VideoStatusResponse | null = null;

        for (let attempt = 0; attempt < 120; attempt += 1) {
          abortController.signal.throwIfAborted();

          latestStatus = await getVideoStatus(startResult.videoId);

          setState((current) => ({
            ...current,
            phase: isTerminalStatus(latestStatus?.status) ? current.phase : 'processing',
            status: latestStatus,
          }));

          if (isReadyStatus(latestStatus.status)) {
            await attachVideoToLesson(lessonId, startResult.videoId);

            void queryClient.invalidateQueries({ queryKey: [...teacherLessonQueryKey, lessonId] });
            void queryClient.invalidateQueries({ queryKey: teacherCoursesQueryKey });
            void queryClient.invalidateQueries({ queryKey: [...teacherVideoQueryKey, startResult.videoId, 'status'] });

            if (courseId) {
              void queryClient.invalidateQueries({ queryKey: [...teacherCoursesQueryKey, courseId] });
              void queryClient.invalidateQueries({ queryKey: [...teacherCoursesQueryKey, courseId, 'content'] });
            }

            let playback: VideoPlaybackResponse | null = null;
            try {
              playback = await getVideoPlayback(startResult.videoId);
            } catch {
              playback = null;
            }

            setState((current) => ({
              ...current,
              phase: 'ready',
              progress: 100,
              status: latestStatus,
              playback,
            }));

            return startResult.videoId;
          }

          if (isFailedStatus(latestStatus.status)) {
            throw new Error(latestStatus.errorMessage ?? 'Обработка видео завершилась ошибкой.');
          }

          await delay(3000, abortController.signal);
        }

        throw new Error('Видео не стало Ready за ожидаемое время. Проверьте статус позже.');
      } catch (error) {
        const message =
          error instanceof DOMException && error.name === 'AbortError' ? 'Загрузка отменена.' : getErrorMessage(error);

        setState((current) => ({
          ...current,
          phase: 'failed',
          error: message,
        }));

        throw error;
      } finally {
        if (abortControllerRef.current === abortController) {
          abortControllerRef.current = null;
        }
      }
    },
    [courseId, lessonId, queryClient],
  );

  return {
    ...state,
    upload,
    cancel,
    reset,
  };
}
