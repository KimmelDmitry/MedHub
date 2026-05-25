import { useQuery } from '@tanstack/react-query';
import { getTeacherCourses, teacherCoursesQueryKey } from '../api/teacherCoursesApi';

export function useTeacherCourses() {
  return useQuery({
    queryKey: teacherCoursesQueryKey,
    queryFn: getTeacherCourses,
  });
}
