import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  archiveCheckpoint,
  checkpointDetailQueryKey,
  videoCheckpointsQueryKey,
} from '../api/teacherCheckpointsApi';

type CheckpointMutationVariables = {
  checkpointId: string;
  videoId: string;
};

export function useArchiveCheckpoint() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ checkpointId }: CheckpointMutationVariables) => archiveCheckpoint(checkpointId),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: videoCheckpointsQueryKey(variables.videoId) });
      void queryClient.invalidateQueries({ queryKey: checkpointDetailQueryKey(variables.checkpointId) });
    },
  });
}
