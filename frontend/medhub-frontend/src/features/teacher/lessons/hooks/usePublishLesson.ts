import { useMutation, useQueryClient } from '@tanstack/react-query';
import { teacherCoursesQueryKey } from '../../courses/api/teacherCoursesApi';
import { publishTeacherLesson, teacherLessonQueryKey } from '../api/teacherLessonsApi';

type LessonMutationVariables = {
  lessonId: string;
  courseId?: string;
};

export function usePublishLesson() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ lessonId }: LessonMutationVariables) => publishTeacherLesson(lessonId),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: teacherCoursesQueryKey });
      void queryClient.invalidateQueries({ queryKey: [...teacherLessonQueryKey, variables.lessonId] });

      if (variables.courseId) {
        void queryClient.invalidateQueries({ queryKey: [...teacherCoursesQueryKey, variables.courseId] });
        void queryClient.invalidateQueries({ queryKey: [...teacherCoursesQueryKey, variables.courseId, 'content'] });
      }
    },
  });
}
