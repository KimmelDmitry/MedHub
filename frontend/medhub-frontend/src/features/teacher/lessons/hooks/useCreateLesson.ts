import { useMutation, useQueryClient } from '@tanstack/react-query';
import { teacherCoursesQueryKey } from '../../courses/api/teacherCoursesApi';
import { createTeacherLesson, type CreateTeacherLessonPayload } from '../api/teacherLessonsApi';

export function useCreateLesson() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: CreateTeacherLessonPayload) => createTeacherLesson(payload),
    onSuccess: (_lessonId, payload) => {
      void queryClient.invalidateQueries({ queryKey: teacherCoursesQueryKey });
      void queryClient.invalidateQueries({ queryKey: [...teacherCoursesQueryKey, payload.courseId] });
      void queryClient.invalidateQueries({ queryKey: [...teacherCoursesQueryKey, payload.courseId, 'content'] });
    },
  });
}
