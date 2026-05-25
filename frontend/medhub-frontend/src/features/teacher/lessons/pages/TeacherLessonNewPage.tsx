import { ArrowLeft } from 'lucide-react';
import { Navigate, useNavigate, useParams } from 'react-router-dom';
import {
  TeacherPlaceholder,
  TeacherShell,
  teacherSecondaryActionLinkClass,
} from '../../components/TeacherShell';
import { useTeacherCourse } from '../../courses/hooks/useTeacherCourse';
import { useTeacherCourseContent } from '../../courses/hooks/useTeacherCourseContent';
import { LessonForm, type LessonFormValues } from '../components/LessonForm';
import { useCreateLesson } from '../hooks/useCreateLesson';
import { getLessonErrorMessage } from '../lib/getLessonErrorMessage';

function getNextLessonOrder(lessons: Array<{ order?: number; orderNumber?: number }>) {
  const maxOrder = lessons.reduce((max, lesson) => {
    const lessonOrder = lesson.orderNumber ?? lesson.order ?? 0;
    return lessonOrder > max ? lessonOrder : max;
  }, 0);

  return maxOrder + 1;
}

export function TeacherLessonNewPage() {
  const { courseId } = useParams();
  const navigate = useNavigate();
  const courseQuery = useTeacherCourse(courseId);
  const contentQuery = useTeacherCourseContent(courseId);
  const createLesson = useCreateLesson();

  if (!courseId) {
    return <Navigate to="/teacher/courses" replace />;
  }

  if (courseQuery.isLoading) {
    return (
      <TeacherShell title="Новый урок" subtitle="Добавление урока в курс">
        <div className="rounded-lg border border-slate-200 bg-white p-8 text-sm text-slate-600 shadow-sm">
          Загрузка курса...
        </div>
      </TeacherShell>
    );
  }

  if (courseQuery.isError || !courseQuery.data) {
    return (
      <TeacherShell title="Новый урок" subtitle="Добавление урока в курс">
        <div className="rounded-lg border border-rose-200 bg-white p-8 text-sm text-rose-700 shadow-sm">
          Не удалось загрузить курс: {getLessonErrorMessage(courseQuery.error) ?? 'неизвестная ошибка'}.
        </div>
      </TeacherShell>
    );
  }

  const course = courseQuery.data;
  const isArchived = course.status.trim().toLowerCase() === 'archived';
  const lessons = contentQuery.data ?? course.lessons ?? [];
  const nextOrder = getNextLessonOrder(lessons);

  const handleSubmit = async (values: LessonFormValues) => {
    await createLesson.mutateAsync({
      courseId,
      ...values,
    });

    navigate(`/teacher/courses/${courseId}`);
  };

  return (
    <TeacherShell title="Новый урок" subtitle={course.title}>
      <div className="flex flex-wrap gap-3 rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
        <button
          type="button"
          onClick={() => navigate(`/teacher/courses/${courseId}`)}
          className={teacherSecondaryActionLinkClass}
        >
          <ArrowLeft className="h-4 w-4" />
          Назад к курсу
        </button>
      </div>

      {isArchived ? (
        <TeacherPlaceholder
          title="Курс в архиве"
          text="В архивный курс нельзя добавлять новые уроки. Верните курс из архива отдельным действием, если такая возможность появится в бэке."
        />
      ) : (
        <LessonForm
          key={nextOrder}
          initialOrder={nextOrder}
          isSubmitting={createLesson.isPending}
          error={getLessonErrorMessage(createLesson.error)}
          onCancel={() => navigate(`/teacher/courses/${courseId}`)}
          onSubmit={handleSubmit}
        />
      )}
    </TeacherShell>
  );
}
