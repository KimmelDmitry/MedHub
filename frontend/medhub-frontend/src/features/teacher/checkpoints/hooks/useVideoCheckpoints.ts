import { useQuery } from '@tanstack/react-query';
import { getVideoCheckpoints, videoCheckpointsQueryKey } from '../api/teacherCheckpointsApi';

export function useVideoCheckpoints(videoId: string | null | undefined, enabled = true) {
  return useQuery({
    queryKey: videoCheckpointsQueryKey(videoId),
    queryFn: () => getVideoCheckpoints(videoId!),
    enabled: Boolean(videoId) && enabled,
  });
}
