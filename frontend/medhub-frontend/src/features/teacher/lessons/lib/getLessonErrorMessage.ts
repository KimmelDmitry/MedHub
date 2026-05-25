import { isAxiosError } from 'axios';

export function getLessonErrorMessage(error: unknown): string | null {
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
      const detail = record.detail ?? record.message ?? record.title ?? record.name ?? record.Name;

      if (typeof detail === 'string') {
        return detail;
      }

      const description = record.description ?? record.code ?? record.Code;

      if (typeof description === 'string') {
        return description;
      }

      return JSON.stringify(data);
    }

    return error.message;
  }

  return error instanceof Error ? error.message : 'Не удалось выполнить действие.';
}
