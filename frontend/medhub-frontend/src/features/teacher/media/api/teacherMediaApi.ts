import { api } from '../../../../app/api/client';
import { ContentType } from '../../../../generated/myApi';

export const teacherVideoQueryKey = ['teacher', 'video'] as const;

export type StartVideoUploadRequest = {
  lessonId: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
};

export type ChunkUploadUrl = {
  partNumber: number;
  uploadUrl: string;
};

export type StartVideoUploadResponse = {
  videoId: string;
  uploadId: string;
  chunkSize: number;
  chunkUploadUrls: ChunkUploadUrl[];
};

export type PartETag = {
  partNumber: number;
  eTag: string;
};

export type CompleteVideoUploadRequest = {
  uploadId: string;
  partETags: PartETag[];
};

export type VideoStatusResponse = {
  videoId: string;
  status: string;
  errorMessage?: string | null;
  durationSeconds?: number | null;
  width?: number | null;
  height?: number | null;
};

export type VideoPlaybackResponse = {
  videoId?: string;
  playbackUrl?: string | null;
  durationSeconds?: number | null;
  duration?: number | null;
  width?: number | null;
  height?: number | null;
  title?: string | null;
};

export async function startVideoUpload(input: StartVideoUploadRequest): Promise<StartVideoUploadResponse> {
  const response = await api.request<StartVideoUploadResponse, unknown>({
    path: '/api/v1/media/videos/start-upload',
    method: 'POST',
    body: input,
    type: ContentType.Json,
  });

  return response.data;
}

export async function completeVideoUpload(videoId: string, input: CompleteVideoUploadRequest): Promise<void> {
  await api.request<void, unknown>({
    path: `/api/v1/media/videos/${videoId}/complete-upload`,
    method: 'POST',
    body: input,
    type: ContentType.Json,
  });
}

export async function getVideoStatus(videoId: string): Promise<VideoStatusResponse> {
  const response = await api.request<VideoStatusResponse, unknown>({
    path: `/api/v1/media/videos/${videoId}/status`,
    method: 'GET',
  });

  return response.data;
}

export async function getVideoPlayback(videoId: string): Promise<VideoPlaybackResponse> {
  const response = await api.request<VideoPlaybackResponse, unknown>({
    path: `/api/v1/media/videos/${videoId}/playback`,
    method: 'GET',
  });

  return response.data;
}

export async function attachVideoToLesson(lessonId: string, videoId: string): Promise<void> {
  await api.request<void, unknown>({
    path: `/api/v1/lessons/${lessonId}/attach-video`,
    method: 'POST',
    body: { videoId },
    type: ContentType.Json,
  });
}
