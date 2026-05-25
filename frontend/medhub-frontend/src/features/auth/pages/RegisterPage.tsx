import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export function RegisterPage() {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isTeacher, setIsTeacher] = useState(false);
  const [teacherRegistrationCode, setTeacherRegistrationCode] = useState('');
  const [message, setMessage] = useState<string | null>(null);

  const { register, isPending, error } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setMessage(null);

    if (password !== confirmPassword) {
      setMessage('Пароли не совпадают.');
      return;
    }

    if (isTeacher && teacherRegistrationCode.trim().length === 0) {
      setMessage('Введите код регистрации преподавателя.');
      return;
    }

    try {
      await register({
        email,
        password,
        firstName,
        lastName,
        teacherRegistrationCode: isTeacher ? teacherRegistrationCode.trim() : undefined,
      });

      navigate(isTeacher ? '/teacher/courses' : '/dashboard');
    } catch {
      setMessage(
        isTeacher
          ? 'Не удалось зарегистрировать преподавателя. Проверьте код доступа и данные аккаунта.'
          : 'Не удалось создать аккаунт. Проверьте данные и попробуйте ещё раз.',
      );
    }
  };

  return (
    <main className="flex min-h-screen items-center justify-center bg-slate-50 p-4">
      <section className="w-full max-w-md rounded-lg border border-slate-200 bg-white p-8 shadow-sm">
        <div className="mb-6">
          <p className="text-sm font-semibold uppercase tracking-[0.16em] text-slate-500">MedHub</p>
          <h1 className="mt-2 text-2xl font-semibold text-slate-950">Создать аккаунт</h1>
          <p className="mt-2 text-sm leading-6 text-slate-600">
            Студент получает доступ к каталогу курсов. Преподавателю нужен код доступа.
          </p>
        </div>

        {message && (
          <div className="mb-4 rounded-lg bg-slate-100 px-4 py-3 text-sm text-slate-900">{message}</div>
        )}
        {error && (
          <div className="mb-4 rounded-lg bg-rose-100 px-4 py-3 text-sm text-rose-900">
            {error.message}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <label className="block">
            <span className="text-sm font-medium text-slate-700">Имя</span>
            <input
              type="text"
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              required
              className="mt-1 block w-full rounded-lg border border-slate-300 px-4 py-3 text-slate-900 focus:border-slate-500 focus:outline-none"
            />
          </label>

          <label className="block">
            <span className="text-sm font-medium text-slate-700">Фамилия</span>
            <input
              type="text"
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              required
              className="mt-1 block w-full rounded-lg border border-slate-300 px-4 py-3 text-slate-900 focus:border-slate-500 focus:outline-none"
            />
          </label>

          <label className="block">
            <span className="text-sm font-medium text-slate-700">Email</span>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="mt-1 block w-full rounded-lg border border-slate-300 px-4 py-3 text-slate-900 focus:border-slate-500 focus:outline-none"
            />
          </label>

          <label className="block">
            <span className="text-sm font-medium text-slate-700">Пароль</span>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="mt-1 block w-full rounded-lg border border-slate-300 px-4 py-3 text-slate-900 focus:border-slate-500 focus:outline-none"
            />
          </label>

          <label className="block">
            <span className="text-sm font-medium text-slate-700">Повторите пароль</span>
            <input
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
              className="mt-1 block w-full rounded-lg border border-slate-300 px-4 py-3 text-slate-900 focus:border-slate-500 focus:outline-none"
            />
          </label>

          <label className="flex items-start gap-3 rounded-lg border border-slate-200 bg-slate-50 p-4">
            <input
              type="checkbox"
              checked={isTeacher}
              onChange={(e) => setIsTeacher(e.target.checked)}
              className="mt-1 h-4 w-4 rounded border-slate-300"
            />
            <span>
              <span className="block text-sm font-medium text-slate-900">Зарегистрироваться как преподаватель</span>
              <span className="mt-1 block text-sm leading-5 text-slate-600">
                Для создания курсов и видеоуроков понадобится код доступа.
              </span>
            </span>
          </label>

          {isTeacher && (
            <label className="block">
              <span className="text-sm font-medium text-slate-700">Код доступа преподавателя</span>
              <input
                type="password"
                value={teacherRegistrationCode}
                onChange={(e) => setTeacherRegistrationCode(e.target.value)}
                required={isTeacher}
                className="mt-1 block w-full rounded-lg border border-slate-300 px-4 py-3 text-slate-900 focus:border-slate-500 focus:outline-none"
              />
            </label>
          )}

          <button
            type="submit"
            disabled={isPending}
            className="inline-flex w-full justify-center rounded-lg bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isPending ? 'Создаём аккаунт...' : 'Создать аккаунт'}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-slate-600">
          Уже есть аккаунт?{' '}
          <Link to="/login" className="font-medium text-slate-900 underline">
            Войти
          </Link>
        </p>
      </section>
    </main>
  );
}
