import { useQuery } from '@tanstack/react-query';
import {
  getMyEnrollments,
  myEnrollmentsQueryKey,
} from '../api/studentCatalogApi';

export function useMyEnrollments(enabled = true) {
  return useQuery({
    queryKey: myEnrollmentsQueryKey,
    queryFn: getMyEnrollments,
    enabled,
  });
}
