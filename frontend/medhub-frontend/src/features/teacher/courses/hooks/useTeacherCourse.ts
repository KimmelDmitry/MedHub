import { useQuery } from '@tanstack/react-query';
import { getTeacherCourse, teacherCoursesQueryKey } from '../api/teacherCoursesApi';

export function useTeacherCourse(courseId: string | undefined) {
  return useQuery({
    queryKey: [...teacherCoursesQueryKey, courseId],
    queryFn: () => getTeacherCourse(courseId!),
    enabled: Boolean(courseId),
  });
}
