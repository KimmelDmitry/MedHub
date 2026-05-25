import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  checkpointDetailQueryKey,
  deleteCheckpoint,
  videoCheckpointsQueryKey,
} from '../api/teacherCheckpointsApi';

type CheckpointMutationVariables = {
  checkpointId: string;
  videoId: string;
};

export function useDeleteCheckpoint() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ checkpointId }: CheckpointMutationVariables) => deleteCheckpoint(checkpointId),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: videoCheckpointsQueryKey(variables.videoId) });
      void queryClient.removeQueries({ queryKey: checkpointDetailQueryKey(variables.checkpointId) });
    },
  });
}
