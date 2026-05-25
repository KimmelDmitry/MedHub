import { Link, NavLink } from 'react-router-dom';
import { BookOpen, LayoutDashboard } from 'lucide-react';
import { useAuth } from '../../auth/hooks/useAuth';

const navItems = [
  { to: '/teacher', label: 'Обзор', icon: LayoutDashboard },
  { to: '/teacher/courses', label: 'Курсы', icon: BookOpen },
];

export const teacherActionLinkClass =
  'inline-flex items-center justify-center gap-2 rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700';

export const teacherSecondaryActionLinkClass =
  'inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 transition hover:border-slate-300 hover:bg-slate-50';

export function TeacherShell({
  title,
  subtitle,
  children,
}: {
  title: string;
  subtitle: string;
  children: React.ReactNode;
}) {
  const { user, role } = useAuth();
  const displayName = user?.firstName || user?.email || 'Преподаватель';

  return (
    <section className="grid gap-6 lg:grid-cols-[240px_1fr]">
      <aside className="self-start rounded-lg border border-slate-200 bg-white p-3 shadow-sm">
        <div className="border-b border-slate-200 px-3 py-4">
          <p className="text-sm font-semibold text-slate-950">{displayName}</p>
          <p className="mt-1 text-xs text-slate-500">{role ?? 'Teacher'}</p>
        </div>
        <nav className="mt-3 space-y-1">
          {navItems.map((item) => {
            const Icon = item.icon;

            return (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.to === '/teacher'}
                className={({ isActive }) =>
                  `flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition ${
                    isActive ? 'bg-slate-900 text-white' : 'text-slate-600 hover:bg-slate-100 hover:text-slate-950'
                  }`
                }
              >
                <Icon className="h-4 w-4" />
                {item.label}
              </NavLink>
            );
          })}
        </nav>
      </aside>

      <div className="min-w-0 space-y-6">
        <header className="flex flex-col gap-4 border-b border-slate-200 pb-6 md:flex-row md:items-end md:justify-between">
          <div>
            <h1 className="text-3xl font-semibold text-slate-950">{title}</h1>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">{subtitle}</p>
          </div>
        </header>
        {children}
      </div>
    </section>
  );
}

export function TeacherStatusCard({
  title,
  description,
  value,
}: {
  title: string;
  description: string;
  value: string;
}) {
  return (
    <article className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h2 className="text-base font-semibold text-slate-950">{title}</h2>
          <p className="mt-2 text-sm leading-6 text-slate-600">{description}</p>
        </div>
        <span className="rounded-md bg-slate-100 px-2.5 py-1 text-xs font-semibold text-slate-600">{value}</span>
      </div>
    </article>
  );
}

export function TeacherPlaceholder({
  title,
  text,
  action,
}: {
  title: string;
  text: string;
  action?: React.ReactNode;
}) {
  return (
    <section className="rounded-lg border border-dashed border-slate-300 bg-white p-8">
      <h2 className="text-xl font-semibold text-slate-950">{title}</h2>
      <p className="mt-3 max-w-2xl text-sm leading-6 text-slate-600">{text}</p>
      {action && <div className="mt-6 flex flex-wrap gap-3">{action}</div>}
    </section>
  );
}

export function TeacherLink({
  to,
  children,
  variant = 'primary',
}: {
  to: string;
  children: React.ReactNode;
  variant?: 'primary' | 'secondary';
}) {
  return (
    <Link to={to} className={variant === 'primary' ? teacherActionLinkClass : teacherSecondaryActionLinkClass}>
      {children}
    </Link>
  );
}
