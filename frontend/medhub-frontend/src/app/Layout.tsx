import { Link, NavLink } from 'react-router-dom';
import { useAuth } from '../features/auth/hooks/useAuth';

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `rounded-full px-4 py-2 text-sm font-medium transition ${
    isActive ? 'bg-slate-900 text-white' : 'text-slate-700 hover:bg-slate-100'
  }`;

export function Layout({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, hasTeacherAccess, logout } = useAuth();
  const panelPath = hasTeacherAccess ? '/teacher' : '/dashboard';
  const panelLabel = hasTeacherAccess ? 'Кабинет' : 'Мои курсы';

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <header className="border-b border-slate-200 bg-white/90 backdrop-blur-sm">
        <div className="mx-auto flex max-w-7xl flex-wrap items-center justify-between gap-3 px-4 py-4 sm:px-6">
          <Link to="/" className="inline-flex items-center gap-3 text-lg font-semibold text-slate-900">
            <span className="inline-flex h-10 w-10 items-center justify-center rounded-lg bg-slate-900 text-sm font-bold text-white">
              M
            </span>
            MedHub
          </Link>

          <nav className="flex flex-wrap items-center gap-2">
            <NavLink to="/" className={navLinkClass}>
              Главная
            </NavLink>
            {isAuthenticated ? (
              <>
                {!hasTeacherAccess && (
                  <NavLink to="/catalog" className={navLinkClass}>
                    Каталог
                  </NavLink>
                )}
                {hasTeacherAccess && (
                  <NavLink to="/teacher/courses" className={navLinkClass}>
                    Курсы
                  </NavLink>
                )}
                <NavLink to={panelPath} className={navLinkClass}>
                  {panelLabel}
                </NavLink>
                <button
                  type="button"
                  onClick={logout}
                  className="rounded-full bg-slate-900 px-4 py-2 text-sm font-medium text-white transition hover:bg-slate-800"
                >
                  Выйти
                </button>
              </>
            ) : (
              <>
                <NavLink to="/login" className={navLinkClass}>
                  Войти
                </NavLink>
                <NavLink to="/register" className={navLinkClass}>
                  Регистрация
                </NavLink>
              </>
            )}
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">{children}</main>
    </div>
  );
}
