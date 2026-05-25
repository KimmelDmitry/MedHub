import { api } from '../../../../app/api/client';
import { ContentType } from '../../../../generated/myApi';

export const teacherCheckpointsQueryKey = ['teacher', 'checkpoints'] as const;

export type VideoCheckpoint = {
  id: string;
  timestampSeconds: number;
  orderNumber: number;
  title?: string | null;
  isRequired: boolean;
  isGraded: boolean;
  status: string;
  questionsCount: number;
};

export type CheckpointAnswerOption = {
  id: string;
  text: string;
  isCorrect: boolean;
};

export type CheckpointQuestion = {
  id: string;
  text: string;
  type: string;
  allowRetry: boolean;
  timeLimitSeconds?: number | null;
  revealCorrectAnswer: boolean;
  correctTextAnswer?: string | null;
  answerOptions: CheckpointAnswerOption[];
};

export type CheckpointDetail = {
  id: string;
  videoId: string;
  timestampSeconds: number;
  orderNumber: number;
  title?: string | null;
  isRequired: boolean;
  isGraded: boolean;
  status: string;
  questions: CheckpointQuestion[];
};

export type CreateCheckpointInput = {
  videoId: string;
  timestampSeconds: number;
  orderNumber: number;
  title?: string | null;
  isRequired: boolean;
  isGraded: boolean;
};

export type UpdateCheckpointInput = {
  checkpointId: string;
  videoId: string;
  title?: string | null;
  timestampSeconds?: number;
  orderNumber?: number;
  isRequired?: boolean;
  isGraded?: boolean;
};

export function videoCheckpointsQueryKey(videoId: string | null | undefined) {
  return [...teacherCheckpointsQueryKey, 'video', videoId] as const;
}

export function checkpointDetailQueryKey(checkpointId: string | null | undefined) {
  return [...teacherCheckpointsQueryKey, 'detail', checkpointId] as const;
}

export async function getVideoCheckpoints(videoId: string): Promise<VideoCheckpoint[]> {
  const response = await api.request<VideoCheckpoint[], unknown>({
    path: `/api/v1/checkpoints/video/${videoId}`,
    method: 'GET',
  });

  return response.data ?? [];
}

export async function getCheckpointDetail(checkpointId: string): Promise<CheckpointDetail> {
  const response = await api.request<CheckpointDetail, unknown>({
    path: `/api/v1/checkpoints/${checkpointId}`,
    method: 'GET',
  });

  return response.data;
}

export async function createCheckpoint(input: CreateCheckpointInput): Promise<string> {
  const response = await api.request<string, unknown>({
    path: '/api/v1/checkpoints',
    method: 'POST',
    body: input,
    type: ContentType.Json,
  });

  return response.data;
}

export async function updateCheckpoint(input: UpdateCheckpointInput): Promise<void> {
  await api.request<void, unknown>({
    path: `/api/v1/checkpoints/${input.checkpointId}`,
    method: 'PATCH',
    body: {
      title: input.title ?? null,
      timestampSeconds: input.timestampSeconds,
      orderNumber: input.orderNumber,
      isRequired: input.isRequired,
      isGraded: input.isGraded,
    },
    type: ContentType.Json,
  });
}

export async function publishCheckpoint(checkpointId: string): Promise<void> {
  await api.request<void, unknown>({
    path: `/api/v1/checkpoints/${checkpointId}/publish`,
    method: 'POST',
  });
}

export async function archiveCheckpoint(checkpointId: string): Promise<void> {
  await api.request<void, unknown>({
    path: `/api/v1/checkpoints/${checkpointId}/archive`,
    method: 'POST',
  });
}

export async function deleteCheckpoint(checkpointId: string): Promise<void> {
  await api.request<void, unknown>({
    path: `/api/v1/checkpoints/${checkpointId}`,
    method: 'DELETE',
  });
}
