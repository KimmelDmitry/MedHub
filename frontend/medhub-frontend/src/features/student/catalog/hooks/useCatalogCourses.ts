import { useQuery } from '@tanstack/react-query';
import {
  catalogCoursesQueryKey,
  getCatalogCourses,
} from '../api/studentCatalogApi';

export function useCatalogCourses(page: number, pageSize: number, enabled = true) {
  return useQuery({
    queryKey: catalogCoursesQueryKey(page, pageSize),
    queryFn: () => getCatalogCourses(page, pageSize),
    enabled,
  });
}
