import { isAxiosError } from 'axios';
import { useNavigate } from 'react-router-dom';
import { TeacherShell } from '../../components/TeacherShell';
import { CourseForm, type CourseFormValues } from '../components/CourseForm';
import { useCreateCourse } from '../hooks/useCreateCourse';

function getErrorMessage(error: unknown): string | null {
  if (!error) {
    return null;
  }

  if (isAxiosError(error)) {
    const data = error.response?.data;

    if (typeof data === 'string') {
      return data;
    }

    if (data && typeof data === 'object') {
      const record = data as Record<string, unknown>;
      const detail = record.detail ?? record.message ?? record.title;

      if (typeof detail === 'string') {
        return detail;
      }

      return JSON.stringify(data);
    }

    return error.message;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return 'Не удалось создать курс.';
}

export function TeacherCourseNewPage() {
  const navigate = useNavigate();
  const createCourse = useCreateCourse();

  const handleSubmit = async (values: CourseFormValues) => {
    await createCourse.mutateAsync(values);
    navigate('/teacher/courses');
  };

  return (
    <TeacherShell title="Новый курс" subtitle="Создание базовой карточки курса">
      <CourseForm
        isSubmitting={createCourse.isPending}
        error={getErrorMessage(createCourse.error)}
        onCancel={() => navigate('/teacher/courses')}
        onSubmit={handleSubmit}
      />
    </TeacherShell>
  );
}
