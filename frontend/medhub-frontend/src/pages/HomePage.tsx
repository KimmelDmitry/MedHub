import { Link } from 'react-router-dom';
import {
  ArrowRight,
  CheckCircle2,
  ClipboardList,
  GraduationCap,
  PlayCircle,
} from 'lucide-react';
import { useAuth } from '../features/auth/hooks/useAuth';

export function HomePage() {
  const { isAuthenticated, hasTeacherAccess } = useAuth();

  const primaryAction = !isAuthenticated
    ? { to: '/login', label: 'Войти' }
    : hasTeacherAccess
      ? { to: '/teacher/courses', label: 'Перейти к курсам' }
      : { to: '/dashboard', label: 'Продолжить обучение' };

  const secondaryAction = !isAuthenticated
    ? { to: '/register', label: 'Создать аккаунт' }
    : hasTeacherAccess
      ? { to: '/teacher', label: 'Кабинет преподавателя' }
      : { to: '/catalog', label: 'Каталог курсов' };

  return (
    <div className="mx-auto max-w-6xl space-y-10">
      <section className="grid gap-8 py-6 lg:grid-cols-[minmax(0,1fr)_420px] lg:items-start">
        <div className="space-y-7">
          <div className="space-y-4">
            <p className="text-sm font-semibold uppercase tracking-[0.18em] text-slate-500">
              MedHub LMS
            </p>
            <h1 className="max-w-3xl text-4xl font-semibold tracking-tight text-slate-950 sm:text-5xl">
              Интерактивные видеолекции для медицинского обучения
            </h1>
            <p className="max-w-2xl text-base leading-7 text-slate-600">
              Студенты смотрят уроки, отвечают на вопросы прямо во время видео и получают
              разбор после завершения. Преподаватель собирает курс из лекций, checkpoint-ов
              и SingleChoice-вопросов в одном рабочем пространстве.
            </p>
          </div>

          <div className="flex flex-col gap-3 sm:flex-row">
            <Link
              to={primaryAction.to}
              className="inline-flex items-center justify-center gap-2 rounded-lg bg-slate-900 px-5 py-3 text-sm font-semibold text-white transition hover:bg-slate-700"
            >
              {primaryAction.label}
              <ArrowRight className="h-4 w-4" />
            </Link>
            <Link
              to={secondaryAction.to}
              className="inline-flex items-center justify-center rounded-lg border border-slate-200 bg-white px-5 py-3 text-sm font-semibold text-slate-800 transition hover:bg-slate-50"
            >
              {secondaryAction.label}
            </Link>
          </div>

          <div className="grid gap-3 sm:grid-cols-3">
            <div className="rounded-lg border border-slate-200 bg-white p-4">
              <p className="text-sm font-semibold text-slate-950">Курс</p>
              <p className="mt-1 text-sm text-slate-600">студент записывается из каталога</p>
            </div>
            <div className="rounded-lg border border-slate-200 bg-white p-4">
              <p className="text-sm font-semibold text-slate-950">Урок</p>
              <p className="mt-1 text-sm text-slate-600">видео открывается после записи</p>
            </div>
            <div className="rounded-lg border border-slate-200 bg-white p-4">
              <p className="text-sm font-semibold text-slate-950">Разбор</p>
              <p className="mt-1 text-sm text-slate-600">результат сохраняется в истории</p>
            </div>
          </div>
        </div>

        <aside className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex items-center gap-3 border-b border-slate-100 pb-4">
            <span className="inline-flex h-11 w-11 items-center justify-center rounded-lg bg-slate-900 text-white">
              <PlayCircle className="h-5 w-5" />
            </span>
            <div>
              <h2 className="font-semibold text-slate-950">Как проходит урок</h2>
              <p className="mt-1 text-sm text-slate-600">
                Видео остается главным экраном, вопросы появляются в нужный момент.
              </p>
            </div>
          </div>

          <ol className="mt-5 space-y-4">
            <li className="flex gap-3">
              <span className="mt-0.5 inline-flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-slate-100 text-xs font-semibold text-slate-700">
                1
              </span>
              <div>
                <p className="text-sm font-medium text-slate-950">Студент открывает урок</p>
                <p className="mt-1 text-sm text-slate-600">
                  Доступ есть только после записи на опубликованный курс.
                </p>
              </div>
            </li>
            <li className="flex gap-3">
              <span className="mt-0.5 inline-flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-slate-100 text-xs font-semibold text-slate-700">
                2
              </span>
              <div>
                <p className="text-sm font-medium text-slate-950">Checkpoint ставит видео на паузу</p>
                <p className="mt-1 text-sm text-slate-600">
                  Будущие проверки не раскрываются заранее и не мешают просмотру.
                </p>
              </div>
            </li>
            <li className="flex gap-3">
              <span className="mt-0.5 inline-flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-slate-100 text-xs font-semibold text-slate-700">
                3
              </span>
              <div>
                <p className="text-sm font-medium text-slate-950">После урока доступен разбор</p>
                <p className="mt-1 text-sm text-slate-600">
                  Студент видит результат, свои ответы и объяснимые состояния попытки.
                </p>
              </div>
            </li>
          </ol>
        </aside>
      </section>

      <section className="grid gap-4 lg:grid-cols-2">
        <article className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex items-start gap-3">
            <span className="inline-flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-slate-100 text-slate-800">
              <GraduationCap className="h-5 w-5" />
            </span>
            <div>
              <h2 className="font-semibold text-slate-950">Для студента</h2>
              <p className="mt-2 text-sm leading-6 text-slate-600">
                Каталог опубликованных курсов, запись на курс, просмотр видеоуроков,
                интерактивные проверки и личный прогресс в кабинете.
              </p>
              <Link
                to={isAuthenticated && !hasTeacherAccess ? '/catalog' : '/login'}
                className="mt-4 inline-flex items-center gap-2 text-sm font-semibold text-slate-900 hover:text-slate-600"
              >
                Открыть обучение
                <ArrowRight className="h-4 w-4" />
              </Link>
            </div>
          </div>
        </article>

        <article className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex items-start gap-3">
            <span className="inline-flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-slate-100 text-slate-800">
              <ClipboardList className="h-5 w-5" />
            </span>
            <div>
              <h2 className="font-semibold text-slate-950">Для преподавателя</h2>
              <p className="mt-2 text-sm leading-6 text-slate-600">
                Создание курсов и уроков, загрузка видео, HLS-проигрывание через backend,
                checkpoint-ы и вопросы без перехода в отдельную медиатеку.
              </p>
              <Link
                to={isAuthenticated && hasTeacherAccess ? '/teacher/courses' : '/login'}
                className="mt-4 inline-flex items-center gap-2 text-sm font-semibold text-slate-900 hover:text-slate-600"
              >
                Открыть преподавание
                <ArrowRight className="h-4 w-4" />
              </Link>
            </div>
          </div>
        </article>
      </section>

      <section className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-slate-950">Что уже закрывает MedHub</h2>
        <div className="mt-4 grid gap-3 md:grid-cols-3">
          {[
            'Публикация курсов, уроков и checkpoint-ов с контролем статусов.',
            'Защищенное HLS-видео и доступ к уроку только после записи на курс.',
            'SingleChoice-вопросы, попытки, завершение урока и разбор результата.',
          ].map((item) => (
            <div key={item} className="flex gap-2 text-sm leading-6 text-slate-600">
              <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600" />
              <span>{item}</span>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
