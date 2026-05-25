import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  checkpointQuestionsQueryKey,
  createQuestion,
  type CreateSingleChoiceQuestionInput,
} from '../api/teacherQuestionsApi';
import { checkpointDetailQueryKey, videoCheckpointsQueryKey } from '../../checkpoints/api/teacherCheckpointsApi';

type CreateQuestionVariables = CreateSingleChoiceQuestionInput & {
  videoId?: string | null;
};

export function useCreateQuestion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreateQuestionVariables) => createQuestion(input),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: checkpointQuestionsQueryKey(variables.checkpointId) });
      void queryClient.invalidateQueries({ queryKey: checkpointDetailQueryKey(variables.checkpointId) });

      if (variables.videoId) {
        void queryClient.invalidateQueries({ queryKey: videoCheckpointsQueryKey(variables.videoId) });
      }
    },
  });
}
