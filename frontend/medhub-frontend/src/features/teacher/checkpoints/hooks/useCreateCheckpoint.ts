import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  createCheckpoint,
  type CreateCheckpointInput,
  videoCheckpointsQueryKey,
} from '../api/teacherCheckpointsApi';

export function useCreateCheckpoint() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreateCheckpointInput) => createCheckpoint(input),
    onSuccess: (_checkpointId, input) => {
      void queryClient.invalidateQueries({ queryKey: videoCheckpointsQueryKey(input.videoId) });
    },
  });
}
