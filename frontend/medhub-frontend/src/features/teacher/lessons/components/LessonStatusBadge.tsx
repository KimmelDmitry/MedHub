const statusClassName: Record<string, string> = {
  draft: 'bg-amber-50 text-amber-700 ring-amber-200',
  published: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  archived: 'bg-slate-100 text-slate-600 ring-slate-200',
};

const statusLabel: Record<string, string> = {
  draft: 'Черновик',
  published: 'Опубликован',
  archived: 'Архив',
};

export function LessonStatusBadge({ status }: { status?: string | null }) {
  const normalizedStatus = status?.trim().toLowerCase() || 'draft';

  return (
    <span
      className={`inline-flex items-center rounded-md px-2.5 py-1 text-xs font-semibold ring-1 ring-inset ${
        statusClassName[normalizedStatus] ?? 'bg-slate-100 text-slate-600 ring-slate-200'
      }`}
    >
      {statusLabel[normalizedStatus] ?? status ?? 'Draft'}
    </span>
  );
}
