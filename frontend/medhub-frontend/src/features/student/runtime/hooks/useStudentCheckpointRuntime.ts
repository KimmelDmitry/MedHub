import { useCallback, useMemo, useRef, useState, type RefObject } from 'react';
import type { HlsVideoPlayerHandle } from '../../../teacher/media/components/HlsVideoPlayer';
import type {
  StudentRuntimeCheckpoint,
  StudentRuntimeQuestion,
} from '../api/studentRuntimeApi';

type ActiveRuntimeCheckpoint = {
  checkpoint: StudentRuntimeCheckpoint;
  question: StudentRuntimeQuestion;
};

type UseStudentCheckpointRuntimeInput = {
  checkpoints: StudentRuntimeCheckpoint[];
  answeredQuestionIds: Set<string>;
  playerRef: RefObject<HlsVideoPlayerHandle | null>;
  enabled: boolean;
};

function isSingleChoice(question: StudentRuntimeQuestion) {
  const normalized = String(question.type).trim().toLowerCase();
  return normalized === 'singlechoice' || normalized === '1';
}

function isPublishedCheckpoint(checkpoint: StudentRuntimeCheckpoint) {
  return !checkpoint.status || checkpoint.status.toLowerCase() === 'published';
}

function getFirstUnansweredQuestion(
  checkpoint: StudentRuntimeCheckpoint,
  answeredQuestionIds: Set<string>,
) {
  return checkpoint.questions.find(
    (question) =>
      isSingleChoice(question) &&
      !answeredQuestionIds.has(question.questionId) &&
      question.answerOptions.length > 0,
  ) ?? null;
}

export function useStudentCheckpointRuntime({
  checkpoints,
  answeredQuestionIds,
  playerRef,
  enabled,
}: UseStudentCheckpointRuntimeInput) {
  const previousTimeRef = useRef<number | null>(null);
  const activeRuntimeRef = useRef<ActiveRuntimeCheckpoint | null>(null);
  const [activeRuntime, setActiveRuntime] = useState<ActiveRuntimeCheckpoint | null>(null);

  const sortedCheckpoints = useMemo(
    () =>
      checkpoints
        .filter(isPublishedCheckpoint)
        .sort(
          (left, right) =>
            left.timestampSeconds - right.timestampSeconds ||
            left.orderNumber - right.orderNumber,
        ),
    [checkpoints],
  );

  const forcePause = useCallback(() => {
    playerRef.current?.pause();

    window.requestAnimationFrame(() => {
      if (activeRuntimeRef.current) {
        playerRef.current?.pause();
      }
    });

    window.setTimeout(() => {
      if (activeRuntimeRef.current) {
        playerRef.current?.pause();
      }
    }, 0);
  }, [playerRef]);

  const keepPausedIfModalOpen = useCallback(() => {
    if (activeRuntimeRef.current) {
      forcePause();
    }
  }, [forcePause]);

  const openCheckpoint = useCallback(
    (checkpoint: StudentRuntimeCheckpoint, question: StudentRuntimeQuestion) => {
      const nextActive = { checkpoint, question };
      activeRuntimeRef.current = nextActive;
      forcePause();
      setActiveRuntime(nextActive);
    },
    [forcePause],
  );

  const handleTimeUpdate = useCallback(
    (currentTime: number) => {
      if (!enabled) {
        previousTimeRef.current = null;
        return;
      }

      const previousTime = previousTimeRef.current;
      previousTimeRef.current = currentTime;

      if (previousTime === null || activeRuntimeRef.current || sortedCheckpoints.length === 0) {
        return;
      }

      if (currentTime < previousTime) {
        return;
      }

      const crossedCheckpoint = sortedCheckpoints.find((checkpoint) => {
        if (
          checkpoint.timestampSeconds <= previousTime ||
          checkpoint.timestampSeconds > currentTime
        ) {
          return false;
        }

        return Boolean(getFirstUnansweredQuestion(checkpoint, answeredQuestionIds));
      });

      if (!crossedCheckpoint) {
        return;
      }

      const question = getFirstUnansweredQuestion(crossedCheckpoint, answeredQuestionIds);

      if (question) {
        openCheckpoint(crossedCheckpoint, question);
      }
    },
    [answeredQuestionIds, enabled, openCheckpoint, sortedCheckpoints],
  );

  const closeActiveCheckpoint = useCallback(() => {
    const checkpoint = activeRuntimeRef.current?.checkpoint;

    if (checkpoint) {
      previousTimeRef.current = checkpoint.timestampSeconds;
    }

    activeRuntimeRef.current = null;
    setActiveRuntime(null);
  }, []);

  const continuePlayback = useCallback(() => {
    closeActiveCheckpoint();

    window.setTimeout(() => {
      playerRef.current?.play();
    }, 0);
  }, [closeActiveCheckpoint, playerRef]);

  const resetRuntime = useCallback(() => {
    previousTimeRef.current = null;
    activeRuntimeRef.current = null;
    setActiveRuntime(null);
  }, []);

  return {
    activeRuntime,
    continuePlayback,
    handlePlay: keepPausedIfModalOpen,
    handlePlaying: keepPausedIfModalOpen,
    handleSeeking: keepPausedIfModalOpen,
    handleSeeked: keepPausedIfModalOpen,
    handleTimeUpdate,
    resetRuntime,
  };
}
