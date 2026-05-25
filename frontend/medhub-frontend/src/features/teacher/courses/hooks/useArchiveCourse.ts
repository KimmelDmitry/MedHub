import { useMutation, useQueryClient } from '@tanstack/react-query';
import { archiveTeacherCourse, teacherCoursesQueryKey } from '../api/teacherCoursesApi';

export function useArchiveCourse() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (courseId: string) => archiveTeacherCourse(courseId),
    onSuccess: (_data, courseId) => {
      void queryClient.invalidateQueries({ queryKey: teacherCoursesQueryKey });
      void queryClient.invalidateQueries({ queryKey: [...teacherCoursesQueryKey, courseId] });
      void queryClient.invalidateQueries({ queryKey: [...teacherCoursesQueryKey, courseId, 'content'] });
    },
  });
}
