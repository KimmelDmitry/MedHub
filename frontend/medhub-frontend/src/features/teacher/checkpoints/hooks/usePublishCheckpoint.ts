import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  checkpointDetailQueryKey,
  publishCheckpoint,
  videoCheckpointsQueryKey,
} from '../api/teacherCheckpointsApi';

type CheckpointMutationVariables = {
  checkpointId: string;
  videoId: string;
};

export function usePublishCheckpoint() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ checkpointId }: CheckpointMutationVariables) => publishCheckpoint(checkpointId),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: videoCheckpointsQueryKey(variables.videoId) });
      void queryClient.invalidateQueries({ queryKey: checkpointDetailQueryKey(variables.checkpointId) });
    },
  });
}
