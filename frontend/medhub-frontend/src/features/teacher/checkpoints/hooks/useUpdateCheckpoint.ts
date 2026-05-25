import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  checkpointDetailQueryKey,
  updateCheckpoint,
  videoCheckpointsQueryKey,
  type UpdateCheckpointInput,
} from '../api/teacherCheckpointsApi';

export function useUpdateCheckpoint() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: UpdateCheckpointInput) => updateCheckpoint(input),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: checkpointDetailQueryKey(variables.checkpointId) });
      void queryClient.invalidateQueries({ queryKey: videoCheckpointsQueryKey(variables.videoId) });
    },
  });
}
