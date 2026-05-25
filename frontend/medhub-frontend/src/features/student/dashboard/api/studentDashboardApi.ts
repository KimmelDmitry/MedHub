import { api } from '../../../../app/api/client';

export const studentDashboardQueryKey = ['student', 'dashboard'] as const;

export type StudentDashboard = {
  enrolledCourses: StudentDashboardCourse[];
  recentAttempts: StudentRecentAttempt[];
};

export type StudentDashboardCourse = {
  enrollmentId: string;
  courseId: string;
  courseTitle: string;
  courseDescription?: string | null;
  enrollmentStatus: string;
  enrolledAtUtc: string;
  publishedLessonsCount: number;
  completedLessonsCount: number;
  progressPercent: number;
  lastActivityAtUtc: string;
  continueLesson?: StudentContinueLesson | null;
  lastCompletedAttempt?: StudentRecentAttempt | null;
};

export type StudentContinueLesson = {
  lessonId: string;
  lessonTitle: string;
  attemptId?: string | null;
  attemptStatus?: string | null;
  score?: number | null;
  updatedAtUtc?: string | null;
};

export type StudentRecentAttempt = {
  attemptId: string;
  courseId: string;
  courseTitle: string;
  lessonId: string;
  lessonTitle: string;
  status: string;
  score: number;
  startedAtUtc: string;
  completedAtUtc?: string | null;
  updatedAtUtc: string;
};

export async function getStudentDashboard(): Promise<StudentDashboard> {
  const response = await api.request<StudentDashboard, unknown>({
    path: '/api/v1/student/dashboard',
    method: 'GET',
  });

  return response.data;
}
