import type { AxiosResponse } from 'axios';
import { api } from '../../../../app/api/client';
import type { CreateCourseRequest } from '../../../../generated/myApi';

export const teacherCoursesQueryKey = ['teacher', 'courses'] as const;

export type CourseStatus = 'Draft' | 'Published' | 'Archived' | string;

export type CourseListItem = {
  id: string;
  title: string;
  description?: string | null;
  status: CourseStatus;
  createdOnUtc: string;
  lessonsCount: number;
};

export type CourseLesson = {
  id: string;
  title: string;
  orderNumber?: number;
  order?: number;
  status?: string;
  contentType?: string | number;
  hasVideo?: boolean;
  videoId?: string | null;
};

export type CourseDetail = {
  id: string;
  title: string;
  description?: string | null;
  status: CourseStatus;
  creatorId?: string;
  createdAt?: string;
  lessons?: CourseLesson[];
};

export async function getTeacherCourses(): Promise<CourseListItem[]> {
  const response = await api.request<CourseListItem[], unknown>({
    path: '/api/v1/courses',
    method: 'GET',
  });

  return response.data ?? [];
}

export async function getTeacherCourse(courseId: string): Promise<CourseDetail> {
  const response = await api.request<CourseDetail, unknown>({
    path: `/api/v1/courses/${courseId}`,
    method: 'GET',
  });

  return response.data;
}

export async function getTeacherCourseContent(courseId: string): Promise<CourseLesson[]> {
  const response = await api.request<CourseLesson[], unknown>({
    path: `/api/v1/courses/${courseId}/content`,
    method: 'GET',
  });

  return response.data ?? [];
}

export async function createTeacherCourse(payload: CreateCourseRequest): Promise<AxiosResponse<void>> {
  return api.api.v1CoursesCreate(payload) as Promise<AxiosResponse<void>>;
}

export async function publishTeacherCourse(courseId: string): Promise<AxiosResponse<void>> {
  return api.api.v1CoursesPublishCreate(courseId) as Promise<AxiosResponse<void>>;
}

export async function archiveTeacherCourse(courseId: string): Promise<AxiosResponse<void>> {
  return api.api.v1CoursesArchiveCreate(courseId) as Promise<AxiosResponse<void>>;
}
