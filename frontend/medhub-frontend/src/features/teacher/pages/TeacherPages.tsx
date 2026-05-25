import { AlertCircle, ArrowRight, BookOpen, FilePlus2, ListChecks } from 'lucide-react';
import {
  TeacherLink,
  TeacherPlaceholder,
  TeacherShell,
  teacherSecondaryActionLinkClass,
} from '../components/TeacherShell';

export function TeacherDashboardPage() {
  return (
    <TeacherShell
      title="Кабинет преподавателя"
      subtitle="Рабочее место для подготовки курсов, видеоуроков и интерактивных checkpoints"
    >
      <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <h2 className="text-xl font-semibold text-slate-950">Продолжить работу</h2>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
              Основной сценарий находится внутри курсов: откройте курс, выберите урок,
              загрузите видео и настройте checkpoints прямо на странице лекции.
            </p>
          </div>
          <div className="flex flex-wrap gap-3">
            <TeacherLink to="/teacher/courses">
              <BookOpen className="h-4 w-4" />
              Открыть курсы
            </TeacherLink>
            <TeacherLink to="/teacher/courses/new" variant="secondary">
              <FilePlus2 className="h-4 w-4" />
              Новый курс
            </TeacherLink>
          </div>
        </div>
      </section>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_360px]">
        <section className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
          <div className="flex items-start gap-4">
            <span className="inline-flex h-10 w-10 items-center justify-center rounded-lg bg-slate-900 text-white">
              <ListChecks className="h-5 w-5" />
            </span>
            <div>
              <h2 className="text-lg font-semibold text-slate-950">Мои курсы</h2>
              <p className="mt-2 text-sm leading-6 text-slate-600">
                Здесь начинается весь teacher flow: курс, уроки, видео, checkpoints и вопросы.
              </p>
              <div className="mt-5">
                <TeacherLink to="/teacher/courses" variant="secondary">
                  Управлять курсами
                  <ArrowRight className="h-4 w-4" />
                </TeacherLink>
              </div>
            </div>
          </div>
        </section>

        <section className="rounded-lg border border-amber-200 bg-amber-50 p-6">
          <div className="flex items-start gap-3">
            <AlertCircle className="mt-0.5 h-5 w-5 text-amber-700" />
            <div>
              <h2 className="text-lg font-semibold text-amber-950">Требуют внимания</h2>
              <ul className="mt-3 space-y-2 text-sm leading-6 text-amber-900">
                <li>Добавьте вопросы к graded checkpoints перед публикацией.</li>
                <li>Публикуйте уроки после загрузки и проверки видео.</li>
                <li>Проверяйте preview перед тем, как студенты начнут проходить урок.</li>
              </ul>
            </div>
          </div>
        </section>
      </div>
    </TeacherShell>
  );
}

export function TeacherLessonsPage() {
  return (
    <TeacherShell title="Уроки" subtitle="Уроки редактируются внутри конкретного курса">
      <TeacherPlaceholder
        title="Откройте курс, чтобы выбрать урок"
        text="Основной редактор лекции находится на странице урока внутри курса. Там доступны видео, checkpoints, вопросы и preview."
        action={
          <TeacherLink to="/teacher/courses">
            <BookOpen className="h-4 w-4" />
            Перейти к курсам
          </TeacherLink>
        }
      />
    </TeacherShell>
  );
}

export function TeacherMediaPage() {
  return (
    <TeacherShell title="Видео" subtitle="Видео загружается из страницы урока">
      <TeacherPlaceholder
        title="Откройте урок, чтобы загрузить видео"
        text="Загрузка, обработка и прикрепление видео выполняются на странице конкретной лекции."
        action={
          <TeacherLink to="/teacher/courses">
            <BookOpen className="h-4 w-4" />
            Перейти к курсам
          </TeacherLink>
        }
      />
    </TeacherShell>
  );
}

export function TeacherAttemptsPage() {
  return (
    <TeacherShell title="Результаты студентов" subtitle="Переход к курсам и урокам преподавателя">
      <section className="rounded-lg border border-dashed border-slate-300 bg-white p-8">
        <h2 className="text-xl font-semibold text-slate-950">Результаты доступны из учебного потока</h2>
        <p className="mt-3 max-w-2xl text-sm leading-6 text-slate-600">
          Сейчас основные действия преподавателя находятся в курсах и уроках. Откройте курс,
          чтобы управлять материалами и интерактивными проверками.
        </p>
        <div className="mt-6">
          <a href="/teacher/courses" className={teacherSecondaryActionLinkClass}>
            <BookOpen className="h-4 w-4" />
            Вернуться к курсам
          </a>
        </div>
      </section>
    </TeacherShell>
  );
}
