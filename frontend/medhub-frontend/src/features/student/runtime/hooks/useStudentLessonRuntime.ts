import { useQuery } from '@tanstack/react-query';
import {
  getStudentLessonRuntime,
  studentLessonRuntimeQueryKey,
} from '../api/studentRuntimeApi';

export function useStudentLessonRuntime(lessonId: string | null | undefined) {
  return useQuery({
    queryKey: studentLessonRuntimeQueryKey(lessonId),
    queryFn: () => getStudentLessonRuntime(lessonId as string),
    enabled: Boolean(lessonId),
    retry: false,
  });
}
