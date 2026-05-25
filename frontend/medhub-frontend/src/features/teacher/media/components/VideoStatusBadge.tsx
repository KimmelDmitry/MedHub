const statusClassName: Record<string, string> = {
  uploading: 'bg-sky-50 text-sky-700 ring-sky-200',
  uploaded: 'bg-blue-50 text-blue-700 ring-blue-200',
  processing: 'bg-violet-50 text-violet-700 ring-violet-200',
  ready: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  failed: 'bg-rose-50 text-rose-700 ring-rose-200',
};

const statusLabel: Record<string, string> = {
  uploading: 'Загрузка',
  uploaded: 'Загружено',
  processing: 'Обработка',
  ready: 'Ready',
  failed: 'Ошибка',
};

export function VideoStatusBadge({ status }: { status?: string | null }) {
  const normalizedStatus = status?.trim().toLowerCase() || 'unknown';

  return (
    <span
      className={`inline-flex items-center rounded-md px-2.5 py-1 text-xs font-semibold ring-1 ring-inset ${
        statusClassName[normalizedStatus] ?? 'bg-slate-100 text-slate-600 ring-slate-200'
      }`}
    >
      {statusLabel[normalizedStatus] ?? status ?? 'Неизвестно'}
    </span>
  );
}
