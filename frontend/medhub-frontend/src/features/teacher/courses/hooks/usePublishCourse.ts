import { useMutation, useQueryClient } from '@tanstack/react-query';
import { publishTeacherCourse, teacherCoursesQueryKey } from '../api/teacherCoursesApi';

export function usePublishCourse() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (courseId: string) => publishTeacherCourse(courseId),
    onSuccess: (_data, courseId) => {
      void queryClient.invalidateQueries({ queryKey: teacherCoursesQueryKey });
      void queryClient.invalidateQueries({ queryKey: [...teacherCoursesQueryKey, courseId] });
      void queryClient.invalidateQueries({ queryKey: [...teacherCoursesQueryKey, courseId, 'content'] });
    },
  });
}
