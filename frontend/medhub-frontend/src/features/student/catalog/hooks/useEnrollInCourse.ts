import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  catalogCourseQueryKey,
  enrollInCourse,
  myEnrollmentsQueryKey,
  studentCatalogQueryKey,
} from '../api/studentCatalogApi';

export function useEnrollInCourse(courseId: string | null | undefined) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => enrollInCourse(courseId as string),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: studentCatalogQueryKey });
      queryClient.invalidateQueries({ queryKey: catalogCourseQueryKey(courseId) });
      queryClient.invalidateQueries({ queryKey: myEnrollmentsQueryKey });
    },
  });
}
