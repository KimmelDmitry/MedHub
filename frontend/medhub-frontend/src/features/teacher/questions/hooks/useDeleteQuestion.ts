import { useMutation, useQueryClient } from '@tanstack/react-query';
import { checkpointQuestionsQueryKey, deleteQuestion } from '../api/teacherQuestionsApi';
import { checkpointDetailQueryKey, videoCheckpointsQueryKey } from '../../checkpoints/api/teacherCheckpointsApi';

type DeleteQuestionVariables = {
  questionId: string;
  checkpointId: string;
  videoId?: string | null;
};

export function useDeleteQuestion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ questionId }: DeleteQuestionVariables) => deleteQuestion(questionId),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: checkpointQuestionsQueryKey(variables.checkpointId) });
      void queryClient.invalidateQueries({ queryKey: checkpointDetailQueryKey(variables.checkpointId) });

      if (variables.videoId) {
        void queryClient.invalidateQueries({ queryKey: videoCheckpointsQueryKey(variables.videoId) });
      }
    },
  });
}
