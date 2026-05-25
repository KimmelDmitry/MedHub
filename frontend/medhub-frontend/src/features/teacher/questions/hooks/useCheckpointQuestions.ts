import { useQuery } from '@tanstack/react-query';
import { checkpointQuestionsQueryKey, getCheckpointQuestions } from '../api/teacherQuestionsApi';

export function useCheckpointQuestions(checkpointId: string | null | undefined) {
  return useQuery({
    queryKey: checkpointQuestionsQueryKey(checkpointId),
    queryFn: () => getCheckpointQuestions(checkpointId as string),
    enabled: Boolean(checkpointId),
  });
}
