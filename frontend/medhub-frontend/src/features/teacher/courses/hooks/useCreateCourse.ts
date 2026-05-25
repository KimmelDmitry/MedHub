import { useMutation, useQueryClient } from '@tanstack/react-query';
import type { CreateCourseRequest } from '../../../../generated/myApi';
import { createTeacherCourse, teacherCoursesQueryKey } from '../api/teacherCoursesApi';

export function useCreateCourse() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateCourseRequest) => createTeacherCourse(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: teacherCoursesQueryKey });
    },
  });
}
