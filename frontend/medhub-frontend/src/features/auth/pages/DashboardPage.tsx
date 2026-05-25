import { Navigate } from 'react-router-dom';
import { StudentDashboardPage } from '../../student/dashboard/pages/StudentDashboardPage';
import { useAuth } from '../hooks/useAuth';

function LoadingState() {
  return (
    <div className="flex min-h-[40vh] items-center justify-center">
      <div className="rounded-lg border border-slate-200 bg-white px-6 py-4 text-sm text-slate-600 shadow-sm">
        Загружаем профиль...
      </div>
    </div>
  );
}

export function DashboardPage() {
  const { hasTeacherAccess, isProfileLoading, isProfileError, logout } = useAuth();

  if (isProfileLoading) {
    return <LoadingState />;
  }

  if (isProfileError) {
    return (
      <section className="mx-auto max-w-2xl rounded-lg border border-rose-200 bg-white p-8 shadow-sm">
        <h1 className="text-2xl font-semibold text-slate-950">Не удалось загрузить профиль</h1>
        <p className="mt-3 text-sm leading-6 text-slate-600">
          Повторите вход, чтобы восстановить доступ к кабинету.
        </p>
        <button
          type="button"
          onClick={logout}
          className="mt-6 rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700"
        >
          Войти заново
        </button>
      </section>
    );
  }

  if (hasTeacherAccess) {
    return <Navigate to="/teacher" replace />;
  }

  return <StudentDashboardPage />;
}
