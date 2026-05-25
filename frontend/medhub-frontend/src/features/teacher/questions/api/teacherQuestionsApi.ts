import { api } from '../../../../app/api/client';
import { ContentType } from '../../../../generated/myApi';

export const teacherQuestionsQueryKey = ['teacher', 'questions'] as const;
export const SINGLE_CHOICE_QUESTION_TYPE = 1;

export type AnswerOptionInput = {
  text: string;
  isCorrect: boolean;
};

export type AnswerOptionDraft = AnswerOptionInput & {
  id?: string;
};

export type AnswerOption = {
  id: string;
  text: string;
  isCorrect: boolean;
};

export type TeacherQuestion = {
  id: string;
  checkpointId: string;
  text: string;
  type: number | string;
  allowRetry: boolean;
  timeLimitSeconds?: number | null;
  revealCorrectAnswer: boolean;
  correctTextAnswer?: string | null;
  answerOptions: AnswerOption[];
};

export type CheckpointQuestionListItem = {
  id: string;
  text: string;
  type: number | string;
  optionsCount: number;
};

export type CreateSingleChoiceQuestionInput = {
  checkpointId: string;
  text: string;
  allowRetry: boolean;
  timeLimitSeconds?: number | null;
  revealCorrectAnswer: boolean;
  answerOptions: AnswerOptionInput[];
};

export type UpdateSingleChoiceQuestionInput = {
  questionId: string;
  checkpointId: string;
  text: string;
  allowRetry: boolean;
  timeLimitSeconds?: number | null;
  revealCorrectAnswer: boolean;
  answerOptions: AnswerOptionDraft[];
  existingAnswerOptions: AnswerOption[];
};

export function checkpointQuestionsQueryKey(checkpointId: string | null | undefined) {
  return [...teacherQuestionsQueryKey, 'checkpoint', checkpointId] as const;
}

export function questionDetailQueryKey(questionId: string | null | undefined) {
  return [...teacherQuestionsQueryKey, 'detail', questionId] as const;
}

export async function getQuestion(questionId: string): Promise<TeacherQuestion> {
  const response = await api.request<TeacherQuestion, unknown>({
    path: `/api/v1/questions/${questionId}`,
    method: 'GET',
  });

  return response.data;
}

export async function getCheckpointQuestions(checkpointId: string): Promise<TeacherQuestion[]> {
  const response = await api.request<CheckpointQuestionListItem[], unknown>({
    path: `/api/v1/checkpoints/${checkpointId}/questions`,
    method: 'GET',
  });

  const items = response.data ?? [];
  return Promise.all(items.map((item) => getQuestion(item.id)));
}

export async function createQuestion(input: CreateSingleChoiceQuestionInput): Promise<TeacherQuestion> {
  const createResponse = await api.request<string, unknown>({
    path: `/api/v1/checkpoints/${input.checkpointId}/questions`,
    method: 'POST',
    body: {
      text: input.text,
      type: SINGLE_CHOICE_QUESTION_TYPE,
      allowRetry: input.allowRetry,
      timeLimitSeconds: input.timeLimitSeconds ?? null,
      revealCorrectAnswer: input.revealCorrectAnswer,
      correctTextAnswer: null,
    },
    type: ContentType.Json,
  });

  const questionId = createResponse.data;

  for (const option of input.answerOptions) {
    await addAnswerOption(questionId, option);
  }

  return getQuestion(questionId);
}

export async function addAnswerOption(questionId: string, input: AnswerOptionInput): Promise<string> {
  const response = await api.request<string, unknown>({
    path: `/api/v1/questions/${questionId}/options`,
    method: 'POST',
    body: input,
    type: ContentType.Json,
  });

  return response.data;
}

export async function updateAnswerOption(
  questionId: string,
  answerOptionId: string,
  input: AnswerOptionInput,
): Promise<void> {
  await api.request<void, unknown>({
    path: `/api/v1/questions/${questionId}/options/${answerOptionId}`,
    method: 'PATCH',
    body: input,
    type: ContentType.Json,
  });
}

export async function deleteAnswerOption(questionId: string, answerOptionId: string): Promise<void> {
  await api.request<void, unknown>({
    path: `/api/v1/questions/${questionId}/options/${answerOptionId}`,
    method: 'DELETE',
  });
}

export async function deleteQuestion(questionId: string): Promise<void> {
  await api.request<void, unknown>({
    path: `/api/v1/questions/${questionId}`,
    method: 'DELETE',
  });
}

export async function updateQuestionText(questionId: string, text: string): Promise<void> {
  await api.request<void, unknown>({
    path: `/api/v1/questions/${questionId}/text`,
    method: 'PATCH',
    body: { text },
    type: ContentType.Json,
  });
}

export async function updateQuestionSettings(
  questionId: string,
  input: Pick<CreateSingleChoiceQuestionInput, 'allowRetry' | 'timeLimitSeconds' | 'revealCorrectAnswer'>,
): Promise<void> {
  await api.request<void, unknown>({
    path: `/api/v1/questions/${questionId}/settings`,
    method: 'PATCH',
    body: {
      allowRetry: input.allowRetry,
      timeLimitSeconds: input.timeLimitSeconds ?? null,
      revealCorrectAnswer: input.revealCorrectAnswer,
      correctTextAnswer: null,
    },
    type: ContentType.Json,
  });
}

export async function updateSingleChoiceQuestion(input: UpdateSingleChoiceQuestionInput): Promise<TeacherQuestion> {
  await updateQuestionText(input.questionId, input.text);
  await updateQuestionSettings(input.questionId, input);

  const nextIds = new Set(input.answerOptions.map((option) => option.id).filter(Boolean));

  for (const existingOption of input.existingAnswerOptions) {
    if (!nextIds.has(existingOption.id)) {
      await deleteAnswerOption(input.questionId, existingOption.id);
    }
  }

  for (const option of input.answerOptions) {
    if (option.id) {
      await updateAnswerOption(input.questionId, option.id, {
        text: option.text,
        isCorrect: option.isCorrect,
      });
    } else {
      await addAnswerOption(input.questionId, {
        text: option.text,
        isCorrect: option.isCorrect,
      });
    }
  }

  return getQuestion(input.questionId);
}
