import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  checkpointQuestionsQueryKey,
  updateSingleChoiceQuestion,
  type UpdateSingleChoiceQuestionInput,
} from '../api/teacherQuestionsApi';
import { checkpointDetailQueryKey, videoCheckpointsQueryKey } from '../../checkpoints/api/teacherCheckpointsApi';

type UpdateQuestionVariables = UpdateSingleChoiceQuestionInput & {
  videoId?: string | null;
};

export function useUpdateSingleChoiceQuestion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: UpdateQuestionVariables) => updateSingleChoiceQuestion(input),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: checkpointQuestionsQueryKey(variables.checkpointId) });
      void queryClient.invalidateQueries({ queryKey: checkpointDetailQueryKey(variables.checkpointId) });

      if (variables.videoId) {
        void queryClient.invalidateQueries({ queryKey: videoCheckpointsQueryKey(variables.videoId) });
      }
    },
  });
}
