import { useQuery } from '@tanstack/react-query';
import {
  getStudentDashboard,
  studentDashboardQueryKey,
} from '../api/studentDashboardApi';

export function useStudentDashboard(enabled = true) {
  return useQuery({
    queryKey: studentDashboardQueryKey,
    queryFn: getStudentDashboard,
    enabled,
  });
}
