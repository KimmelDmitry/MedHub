import { useCallback, useMemo, useRef, useState, type RefObject } from 'react';
import { useQuery } from '@tanstack/react-query';
import type { HlsVideoPlayerHandle } from '../../../media/components/HlsVideoPlayer';
import {
  getCheckpointDetail,
  getVideoCheckpoints,
  type CheckpointDetail,
} from '../../api/teacherCheckpointsApi';

function isPublished(status: string) {
  return status.trim().toLowerCase() === 'published';
}

export function useCheckpointPreview({
  videoId,
  playerRef,
  enabled,
}: {
  videoId?: string | null;
  playerRef: RefObject<HlsVideoPlayerHandle | null>;
  enabled: boolean;
}) {
  const triggeredIdsRef = useRef<Set<string>>(new Set());
  const activeCheckpointRef = useRef<CheckpointDetail | null>(null);
  const previousTimeRef = useRef<number | null>(null);
  const [triggeredCount, setTriggeredCount] = useState(0);
  const [activeCheckpoint, setActiveCheckpoint] = useState<CheckpointDetail | null>(null);

  const checkpointsQuery = useQuery({
    queryKey: ['teacher', 'checkpoints', 'preview', videoId],
    queryFn: async () => {
      const items = await getVideoCheckpoints(videoId as string);
      const publishedItems = items
        .filter((checkpoint) => isPublished(checkpoint.status))
        .sort((left, right) => left.timestampSeconds - right.timestampSeconds || left.orderNumber - right.orderNumber);

      return Promise.all(publishedItems.map((checkpoint) => getCheckpointDetail(checkpoint.id)));
    },
    enabled: enabled && Boolean(videoId),
  });

  const checkpoints = useMemo(
    () => checkpointsQuery.data ?? [],
    [checkpointsQuery.data],
  );

  const forcePause = useCallback(() => {
    playerRef.current?.pause();

    window.requestAnimationFrame(() => {
      if (activeCheckpointRef.current) {
        playerRef.current?.pause();
      }
    });

    window.setTimeout(() => {
      if (activeCheckpointRef.current) {
        playerRef.current?.pause();
      }
    }, 0);
  }, [playerRef]);

  const keepPausedIfModalOpen = useCallback(() => {
    if (activeCheckpointRef.current) {
      forcePause();
    }
  }, [forcePause]);

  const handleTimeUpdate = useCallback((currentTime: number) => {
    if (!enabled) {
      previousTimeRef.current = null;
      return;
    }

    const previousTime = previousTimeRef.current;
    previousTimeRef.current = currentTime;

    if (previousTime === null || activeCheckpointRef.current || checkpoints.length === 0) {
      return;
    }

    if (currentTime < previousTime) {
      return;
    }

    const checkpoint = checkpoints.find(
      (item) =>
        !triggeredIdsRef.current.has(item.id) &&
        item.timestampSeconds > previousTime &&
        item.timestampSeconds <= currentTime,
    );

    if (!checkpoint) {
      return;
    }

    activeCheckpointRef.current = checkpoint;
    forcePause();
    setActiveCheckpoint(checkpoint);
  }, [checkpoints, enabled, forcePause]);

  const continuePlayback = () => {
    const checkpoint = activeCheckpointRef.current;

    if (checkpoint) {
      triggeredIdsRef.current.add(checkpoint.id);
      setTriggeredCount(triggeredIdsRef.current.size);
      previousTimeRef.current = checkpoint.timestampSeconds;
    }

    activeCheckpointRef.current = null;
    setActiveCheckpoint(null);
    window.setTimeout(() => {
      playerRef.current?.play();
    }, 0);
  };

  const resetPreview = () => {
    triggeredIdsRef.current.clear();
    activeCheckpointRef.current = null;
    previousTimeRef.current = null;
    setTriggeredCount(0);
    setActiveCheckpoint(null);
  };

  return {
    activeCheckpoint,
    checkpoints,
    checkpointsQuery,
    continuePlayback,
    handlePlay: keepPausedIfModalOpen,
    handlePlaying: keepPausedIfModalOpen,
    handleSeeked: keepPausedIfModalOpen,
    handleSeeking: keepPausedIfModalOpen,
    handleTimeUpdate,
    resetPreview,
    triggeredCount,
  };
}

export type CheckpointPreviewState = ReturnType<typeof useCheckpointPreview>;
