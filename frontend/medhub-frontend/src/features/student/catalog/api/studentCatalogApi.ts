import { api } from '../../../../app/api/client';

export const studentCatalogQueryKey = ['student', 'catalog'] as const;

export type PagedResponse<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type CatalogCourseListItem = {
  id: string;
  title: string;
  description?: string | null;
  lessonsCount: number;
  publishedLessonsCount: number;
  hasVideo: boolean;
  checkpointsCount: number;
  createdAt: string;
  isEnrolled: boolean;
  enrollmentStatus?: string | null;
};

export type CatalogLessonItem = {
  id: string;
  title: string;
  order: number;
  contentType: string;
  hasVideo: boolean;
  videoReady: boolean;
  durationSeconds?: number | null;
  checkpointsCount: number;
};

export type CatalogCourse = {
  id: string;
  title: string;
  description?: string | null;
  isEnrolled: boolean;
  enrollmentStatus?: string | null;
  lessons: CatalogLessonItem[];
};

export type EnrollmentResponse = {
  enrollmentId: string;
  courseId: string;
  status: string;
  enrolledAtUtc: string;
};

export type MyEnrollment = {
  enrollmentId: string;
  courseId: string;
  courseTitle: string;
  courseDescription?: string | null;
  status: string;
  enrolledAtUtc: string;
  completedAtUtc?: string | null;
  lessonsCount: number;
  completedLessonsCount: number;
  lastAttemptAt?: string | null;
};

export function catalogCoursesQueryKey(page: number, pageSize: number) {
  return [...studentCatalogQueryKey, 'courses', page, pageSize] as const;
}

export function catalogCourseQueryKey(courseId: string | null | undefined) {
  return [...studentCatalogQueryKey, 'course', courseId] as const;
}

export const myEnrollmentsQueryKey = [...studentCatalogQueryKey, 'my-enrollments'] as const;

export async function getCatalogCourses(
  page = 1,
  pageSize = 20,
): Promise<PagedResponse<CatalogCourseListItem>> {
  const response = await api.request<PagedResponse<CatalogCourseListItem>, unknown>({
    path: `/api/v1/catalog/courses?page=${page}&pageSize=${pageSize}`,
    method: 'GET',
  });

  return response.data;
}

export async function getCatalogCourse(courseId: string): Promise<CatalogCourse> {
  const response = await api.request<CatalogCourse, unknown>({
    path: `/api/v1/catalog/courses/${courseId}`,
    method: 'GET',
  });

  return response.data;
}

export async function enrollInCourse(courseId: string): Promise<EnrollmentResponse> {
  const response = await api.request<EnrollmentResponse, unknown>({
    path: `/api/v1/catalog/courses/${courseId}/enroll`,
    method: 'POST',
  });

  return response.data;
}

export async function getMyEnrollments(): Promise<MyEnrollment[]> {
  const response = await api.request<MyEnrollment[], unknown>({
    path: '/api/v1/student/enrollments',
    method: 'GET',
  });

  return response.data;
}
