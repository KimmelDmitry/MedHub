import { useQuery } from '@tanstack/react-query';
import {
  catalogCourseQueryKey,
  getCatalogCourse,
} from '../api/studentCatalogApi';

export function useCatalogCourse(courseId: string | null | undefined) {
  return useQuery({
    queryKey: catalogCourseQueryKey(courseId),
    queryFn: () => getCatalogCourse(courseId as string),
    enabled: Boolean(courseId),
  });
}
