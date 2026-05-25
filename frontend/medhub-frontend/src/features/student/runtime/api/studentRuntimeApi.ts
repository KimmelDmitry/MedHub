import { api } from '../../../../app/api/client';
import { ContentType } from '../../../../generated/myApi';

export const studentRuntimeQueryKey = ['student', 'runtime'] as const;

export type StudentLessonRuntime = {
  lessonId: string;
  courseId: string;
  courseTitle?: string | null;
  lessonTitle: string;
  video: {
    videoId: string;
    hlsMasterUrl: string;
    durationSeconds?: number | null;
    width?: number | null;
    height?: number | null;
    title?: string | null;
  };
  checkpoints: StudentRuntimeCheckpoint[];
  activeAttempt: StudentRuntimeActiveAttempt | null;
};

export type StudentRuntimeActiveAttempt = {
  attemptId: string;
  status: string;
  answeredQuestionIds: string[];
};

export type StudentRuntimeCheckpoint = {
  checkpointId: string;
  timestampSeconds: number;
  orderNumber: number;
  title?: string | null;
  status?: string | null;
  isRequired: boolean;
  isGraded: boolean;
  questions: StudentRuntimeQuestion[];
};

export type StudentRuntimeQuestion = {
  questionId: string;
  text: string;
  type: string;
  allowRetry: boolean;
  timeLimitSeconds?: number | null;
  revealCorrectAnswer: boolean;
  answerOptions: StudentRuntimeAnswerOption[];
};

export type StudentRuntimeAnswerOption = {
  id: string;
  text: string;
};

export type StartAttemptResponse = {
  attemptId: string;
  lessonId: string;
  startedAt: string;
  status: string;
};

export type SubmitCheckpointAnswerPayload = {
  questionId: string;
  selectedOptionIds: string[];
  textAnswer: string | null;
};

export type SubmitCheckpointAnswerResponse = {
  answerId: string;
  isCorrect: boolean;
  currentScore: number;
};

export type CompleteAttemptResponse = {
  attemptId: string;
  finalScore: number;
  completedAt: string;
  status: string;
};

export type AttemptResultResponse = {
  attemptId: string;
  lessonId: string;
  status: string;
  score: number;
  startedAt: string;
  completedAt?: string | null;
  totalQuestions: number;
  correctAnswers: number;
  answers: AttemptAnswerReview[];
};

export type AttemptAnswerReview = {
  questionId: string;
  checkpointId: string;
  checkpointTitle?: string | null;
  timestampSeconds: number;
  questionText: string;
  type: string;
  selectedOptions: AnswerOptionReview[];
  isCorrect: boolean;
  revealCorrectAnswer: boolean;
  correctOptions: AnswerOptionReview[];
  textAnswer?: string | null;
  requiresManualReview: boolean;
};

export type AnswerOptionReview = {
  id: string;
  text: string;
};

export function studentLessonRuntimeQueryKey(lessonId: string | null | undefined) {
  return [...studentRuntimeQueryKey, 'lesson', lessonId] as const;
}

export function attemptResultQueryKey(attemptId: string | null | undefined) {
  return ['student', 'attempt-result', attemptId] as const;
}

export async function getStudentLessonRuntime(lessonId: string): Promise<StudentLessonRuntime> {
  const response = await api.request<StudentLessonRuntime, unknown>({
    path: `/api/v1/student/lessons/${lessonId}/runtime`,
    method: 'GET',
  });

  return response.data;
}

export async function startAttempt(lessonId: string): Promise<StartAttemptResponse> {
  const response = await api.request<StartAttemptResponse, unknown>({
    path: `/api/v1/lessons/${lessonId}/attempts/start`,
    method: 'POST',
  });

  return response.data;
}

export async function getActiveAttempt(lessonId: string): Promise<StudentRuntimeActiveAttempt> {
  const response = await api.request<StudentRuntimeActiveAttempt, unknown>({
    path: `/api/v1/lessons/${lessonId}/attempts/active`,
    method: 'GET',
  });

  return response.data;
}

export async function submitCheckpointAnswer(
  attemptId: string,
  payload: SubmitCheckpointAnswerPayload,
): Promise<SubmitCheckpointAnswerResponse> {
  const response = await api.request<SubmitCheckpointAnswerResponse, unknown>({
    path: `/api/v1/attempts/${attemptId}/answers`,
    method: 'POST',
    body: payload,
    type: ContentType.Json,
  });

  return response.data;
}

export async function completeAttempt(attemptId: string): Promise<CompleteAttemptResponse> {
  const response = await api.request<CompleteAttemptResponse, unknown>({
    path: `/api/v1/attempts/${attemptId}/complete`,
    method: 'POST',
  });

  return response.data;
}

export async function getAttemptResult(attemptId: string): Promise<AttemptResultResponse> {
  const response = await api.request<AttemptResultResponse, unknown>({
    path: `/api/v1/attempts/${attemptId}/result`,
    method: 'GET',
  });

  return response.data;
}
