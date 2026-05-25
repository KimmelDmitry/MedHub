import type { AxiosResponse } from 'axios';
import { api } from '../../../../app/api/client';
import type { CreateLessonRequest, LessonContentType as GeneratedLessonContentType } from '../../../../generated/myApi';

export const teacherLessonQueryKey = ['teacher', 'lesson'] as const;

export type LessonContentTypeValue = 1 | 2 | 3;
export type LessonStatus = 'Draft' | 'Published' | 'Archived' | string;

export type TeacherLessonDetail = {
  id: string;
  courseId: string;
  title: string;
  orderNumber: number;
  contentType: string;
  contentUrl?: string | null;
  status: LessonStatus;
  videoId?: string | null;
  createdAt: string;
};

export type CreateTeacherLessonPayload = {
  courseId: string;
  title: string;
  order: number;
  contentType: LessonContentTypeValue;
  contentUrl?: string | null;
};

export const lessonContentTypeOptions: Array<{
  value: LessonContentTypeValue;
  label: string;
  description: string;
}> = [
  {
    value: 2,
    label: 'Текст',
    description: 'Текстовый материал или ссылка для урока.',
  },
  {
    value: 1,
    label: 'Видео',
    description: 'Видео добавляется на странице урока после создания.',
  },
  {
    value: 3,
    label: 'Квиз',
    description: 'Вопросы настраиваются через checkpoints внутри видеоурока.',
  },
];

export function formatLessonContentType(contentType?: string | number | null): string {
  if (contentType === 1 || String(contentType).toLowerCase() === 'video') {
    return 'Видео';
  }

  if (contentType === 2 || String(contentType).toLowerCase() === 'text') {
    return 'Текст';
  }

  if (contentType === 3 || String(contentType).toLowerCase() === 'quiz') {
    return 'Квиз';
  }

  return contentType ? String(contentType) : 'Контент';
}

export async function getTeacherLesson(lessonId: string): Promise<TeacherLessonDetail> {
  const response = (await api.api.v1LessonsDetail(lessonId)) as unknown as AxiosResponse<TeacherLessonDetail>;

  return response.data;
}

export async function createTeacherLesson(payload: CreateTeacherLessonPayload): Promise<string | null> {
  const request: CreateLessonRequest = {
    courseId: payload.courseId,
    title: payload.title,
    order: payload.order,
    contentType: payload.contentType as GeneratedLessonContentType,
    contentUrl: payload.contentUrl ?? null,
  };

  const response = (await api.api.v1LessonsCreate(request)) as unknown as AxiosResponse<string>;

  return response.data ?? null;
}

export async function publishTeacherLesson(lessonId: string): Promise<AxiosResponse<void>> {
  return api.api.v1LessonsPublishCreate(lessonId) as Promise<AxiosResponse<void>>;
}

export async function archiveTeacherLesson(lessonId: string): Promise<AxiosResponse<void>> {
  return api.api.v1LessonsArchiveCreate(lessonId) as Promise<AxiosResponse<void>>;
}
