import { useQuery } from '@tanstack/react-query';
import { getTeacherCourseContent, teacherCoursesQueryKey } from '../api/teacherCoursesApi';

export function useTeacherCourseContent(courseId: string | undefined) {
  return useQuery({
    queryKey: [...teacherCoursesQueryKey, courseId, 'content'],
    queryFn: () => getTeacherCourseContent(courseId!),
    enabled: Boolean(courseId),
  });
}
