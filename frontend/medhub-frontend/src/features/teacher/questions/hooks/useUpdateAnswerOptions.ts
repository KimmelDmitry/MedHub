import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  addAnswerOption,
  checkpointQuestionsQueryKey,
  deleteAnswerOption,
  updateAnswerOption,
  type AnswerOptionInput,
} from '../api/teacherQuestionsApi';
import { checkpointDetailQueryKey, videoCheckpointsQueryKey } from '../../checkpoints/api/teacherCheckpointsApi';

type BaseVariables = {
  questionId: string;
  checkpointId: string;
  videoId?: string | null;
};

function invalidateQuestionState(
  queryClient: ReturnType<typeof useQueryClient>,
  checkpointId: string,
  videoId?: string | null,
) {
  void queryClient.invalidateQueries({ queryKey: checkpointQuestionsQueryKey(checkpointId) });
  void queryClient.invalidateQueries({ queryKey: checkpointDetailQueryKey(checkpointId) });

  if (videoId) {
    void queryClient.invalidateQueries({ queryKey: videoCheckpointsQueryKey(videoId) });
  }
}

export function useAddAnswerOption() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ questionId, input }: BaseVariables & { input: AnswerOptionInput }) =>
      addAnswerOption(questionId, input),
    onSuccess: (_data, variables) => {
      invalidateQuestionState(queryClient, variables.checkpointId, variables.videoId);
    },
  });
}

export function useUpdateAnswerOption() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ questionId, answerOptionId, input }: BaseVariables & { answerOptionId: string; input: AnswerOptionInput }) =>
      updateAnswerOption(questionId, answerOptionId, input),
    onSuccess: (_data, variables) => {
      invalidateQuestionState(queryClient, variables.checkpointId, variables.videoId);
    },
  });
}

export function useDeleteAnswerOption() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ questionId, answerOptionId }: BaseVariables & { answerOptionId: string }) =>
      deleteAnswerOption(questionId, answerOptionId),
    onSuccess: (_data, variables) => {
      invalidateQuestionState(queryClient, variables.checkpointId, variables.videoId);
    },
  });
}
