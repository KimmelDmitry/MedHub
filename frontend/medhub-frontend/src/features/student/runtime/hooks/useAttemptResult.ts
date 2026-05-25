import { useQuery } from '@tanstack/react-query';
import {
  attemptResultQueryKey,
  getAttemptResult,
} from '../api/studentRuntimeApi';

export function useAttemptResult(
  attemptId: string | null | undefined,
  enabled: boolean,
) {
  return useQuery({
    queryKey: attemptResultQueryKey(attemptId),
    queryFn: () => getAttemptResult(attemptId as string),
    enabled: Boolean(attemptId) && enabled,
    retry: false,
  });
}
