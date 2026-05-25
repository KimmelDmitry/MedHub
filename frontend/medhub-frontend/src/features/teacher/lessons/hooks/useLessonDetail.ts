import { useQuery } from '@tanstack/react-query';
import { getTeacherLesson, teacherLessonQueryKey } from '../api/teacherLessonsApi';

export function useLessonDetail(lessonId: string | undefined) {
  return useQuery({
    queryKey: [...teacherLessonQueryKey, lessonId],
    queryFn: () => getTeacherLesson(lessonId!),
    enabled: Boolean(lessonId),
  });
}
