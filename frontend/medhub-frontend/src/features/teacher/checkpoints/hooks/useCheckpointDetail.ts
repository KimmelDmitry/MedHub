import { useQuery } from '@tanstack/react-query';
import { checkpointDetailQueryKey, getCheckpointDetail } from '../api/teacherCheckpointsApi';

export function useCheckpointDetail(checkpointId: string | null | undefined) {
  return useQuery({
    queryKey: checkpointDetailQueryKey(checkpointId),
    queryFn: () => getCheckpointDetail(checkpointId as string),
    enabled: Boolean(checkpointId),
  });
}
