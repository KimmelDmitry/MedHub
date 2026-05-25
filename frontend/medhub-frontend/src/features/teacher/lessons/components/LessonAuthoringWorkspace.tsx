import { useMemo, useState, type RefObject } from 'react';
import type { TeacherLessonDetail } from '../api/teacherLessonsApi';
import { VideoUploadPanel } from '../../media/components/VideoUploadPanel';
import type { HlsVideoPlayerHandle } from '../../media/components/HlsVideoPlayer';
import { CheckpointAuthoringPanel } from '../../checkpoints/components/CheckpointAuthoringPanel';
import { CheckpointTimeline } from '../../checkpoints/components/CheckpointTimeline';
import type { CheckpointPreviewState } from '../../checkpoints/preview/hooks/useCheckpointPreview';
import { useCreateCheckpoint } from '../../checkpoints/hooks/useCreateCheckpoint';
import { useVideoCheckpoints } from '../../checkpoints/hooks/useVideoCheckpoints';
import type { VideoCheckpoint } from '../../checkpoints/api/teacherCheckpointsApi';

type LessonAuthoringWorkspaceProps = {
  lesson: TeacherLessonDetail;
  isVideoReady: boolean;
  playerRef: RefObject<HlsVideoPlayerHandle | null>;
  isPreviewEnabled: boolean;
  onPreviewEnabledChange: (enabled: boolean) => void;
  preview: CheckpointPreviewState;
};

export function LessonAuthoringWorkspace({
  lesson,
  isVideoReady,
  playerRef,
  isPreviewEnabled,
  onPreviewEnabledChange,
  preview,
}: LessonAuthoringWorkspaceProps) {
  const [selectedCheckpointId, setSelectedCheckpointId] = useState<string | null>(null);
  const checkpointsQuery = useVideoCheckpoints(lesson.videoId);
  const createCheckpoint = useCreateCheckpoint();
  const checkpoints = useMemo(
    () =>
      [...(checkpointsQuery.data ?? [])].sort(
        (left, right) =>
          left.timestampSeconds - right.timestampSeconds || left.orderNumber - right.orderNumber,
      ),
    [checkpointsQuery.data],
  );
  const nextOrderNumber = checkpoints.reduce((max, checkpoint) => Math.max(max, checkpoint.orderNumber), 0) + 1;
  const safeSelectedCheckpointId =
    selectedCheckpointId && checkpoints.some((checkpoint) => checkpoint.id === selectedCheckpointId)
      ? selectedCheckpointId
      : null;

  const getCurrentVideoTime = () => {
    const currentTime = playerRef.current?.getCurrentTime();
    return typeof currentTime === 'number' && Number.isFinite(currentTime) ? currentTime : null;
  };

  const handleSelectCheckpoint = (checkpoint: VideoCheckpoint) => {
    setSelectedCheckpointId(checkpoint.id);
    playerRef.current?.seekTo(checkpoint.timestampSeconds, { autoplay: false });
  };

  const handleCreateAtCurrentTime = () => {
    if (!lesson.videoId) {
      return;
    }

    const currentTime = getCurrentVideoTime();

    createCheckpoint.mutate(
      {
        videoId: lesson.videoId,
        timestampSeconds: Math.max(0, Math.floor(currentTime ?? 0)),
        orderNumber: nextOrderNumber,
        title: null,
        isRequired: true,
        isGraded: true,
      },
      {
        onSuccess: (checkpointId) => {
          setSelectedCheckpointId(checkpointId);
        },
      },
    );
  };

  return (
    <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_400px] xl:items-start">
      <div className="min-w-0 space-y-6">
        <VideoUploadPanel
          lessonId={lesson.id}
          courseId={lesson.courseId}
          videoId={lesson.videoId}
          playerRef={playerRef}
          onTimeUpdate={preview.handleTimeUpdate}
          onPlay={preview.handlePlay}
          onPlaying={preview.handlePlaying}
          onSeeking={preview.handleSeeking}
          onSeeked={preview.handleSeeked}
        />

        {lesson.videoId && isVideoReady && (
          <CheckpointTimeline
            checkpoints={checkpoints}
            selectedCheckpointId={safeSelectedCheckpointId}
            isLoading={checkpointsQuery.isLoading}
            isError={checkpointsQuery.isError}
            isCreating={createCheckpoint.isPending}
            onSelect={handleSelectCheckpoint}
            onCreateAtCurrentTime={handleCreateAtCurrentTime}
          />
        )}
      </div>

      {lesson.videoId && isVideoReady && (
        <div className="min-w-0">
          <CheckpointAuthoringPanel
            videoId={lesson.videoId}
            checkpoints={checkpoints}
            selectedCheckpointId={safeSelectedCheckpointId}
            onSelectedCheckpointIdChange={setSelectedCheckpointId}
            onUseCurrentTime={getCurrentVideoTime}
            playerRef={playerRef}
            isPreviewEnabled={isPreviewEnabled}
            onPreviewEnabledChange={onPreviewEnabledChange}
            preview={preview}
          />
        </div>
      )}
    </div>
  );
}
