import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../features/auth/hooks/useAuth';

interface ProtectedRouteProps {
  children: ReactNode;
  teacherOnly?: boolean;
}

export function ProtectedRoute({ children, teacherOnly = false }: ProtectedRouteProps) {
  const { isAuthenticated, isProfileLoading, hasTeacherAccess } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (isProfileLoading) {
    return (
      <div className="flex min-h-[40vh] items-center justify-center">
        <div className="rounded-lg border border-slate-200 bg-white px-6 py-4 text-sm text-slate-600 shadow-sm">
          Загрузка профиля...
        </div>
      </div>
    );
  }

  if (teacherOnly && !hasTeacherAccess) {
    return <Navigate to="/dashboard" replace />;
  }

  return children;
}
